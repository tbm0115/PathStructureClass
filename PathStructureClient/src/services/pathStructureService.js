const { EventEmitter } = require('events');
const fs = require('fs');
const path = require('path');

/**
 * @typedef {import('../dtos/PathMatchDto').PathMatchDto} PathMatchDto
 * @typedef {import('../dtos/PathChangedNotificationParams').PathChangedNotificationParams} PathChangedNotificationParams
 */

/**
 * @typedef {object} DirectoryEntry
 * @property {string} name
 * @property {string} fullPath
 * @property {boolean} isDirectory
 * @property {boolean} isFile
 */

/**
 * @typedef {object} DirectoryEntries
 * @property {DirectoryEntry[]} entries
 * @property {string|null} error
 */

/**
 * @typedef {object} ChildValidationException
 * @property {'warning'|'error'|'fatal'} severity
 * @property {string} message
 */

/**
 * @typedef {object} ChildStructureState
 * @property {string} name
 * @property {string} displayPath
 * @property {string|null} literalPath
 * @property {string} flavorText
 * @property {string} pattern
 * @property {boolean} isRequired
 * @property {string|null} icon
 * @property {string|null} backgroundColor
 * @property {string|null} foregroundColor
 * @property {string[]} matchingPaths
 * @property {string|null} trackedFolder
 * @property {boolean} exists
 * @property {boolean} isFile
 * @property {ChildValidationException[]} exceptions
 * @property {'warning'|'error'|'fatal'|null} severity
 */

const severityOrder = {
  warning: 1,
  error: 2,
  fatal: 3
};

const getSeverityRank = (severity) => severityOrder[severity] || 0;

const normalizePath = (value) => (value ? value.replace(/\//g, '\\') : value);

const buildGroupCaptureRegex = (pattern) => {
  if (!pattern) {
    return null;
  }
  const groupRegex = /\(\?<[^>]+>/g;
  const matches = [...pattern.matchAll(groupRegex)];
  if (matches.length === 0) {
    return null;
  }
  const lastGroup = matches[matches.length - 1];
  const startIndex = lastGroup.index ?? 0;
  const closeIndex = pattern.indexOf(')', startIndex);
  if (closeIndex === -1) {
    return null;
  }
  const prefix = pattern.slice(0, closeIndex + 1);
  return `${prefix}.*$`;
};

const tryMatchAgainstPath = (regex, pathValue) => {
  if (!regex || !pathValue) {
    return null;
  }
  return regex.exec(pathValue) || regex.exec(normalizePath(pathValue));
};

const renderTemplate = (template, match) => {
  if (!template) {
    return '';
  }
  if (!match?.groups) {
    return template;
  }
  return template.replace(/\{\{\s*([^}\s]+)\s*\}\}/g, (_token, key) => {
    const value = match.groups[key];
    return value ?? '';
  });
};

const renderTemplateWithVariables = (template, variables) => {
  if (!template) {
    return '';
  }
  if (!variables || typeof variables !== 'object') {
    return template;
  }
  return template.replace(/\{\{\s*([^}\s]+)\s*\}\}/g, (_token, key) => {
    const value = variables[key];
    return value ?? '';
  });
};

const extractGroupNames = (pattern) => {
  if (!pattern) {
    return [];
  }
  const groups = [];
  const regex = /\(\?<([^>]+)>/g;
  let match;
  while ((match = regex.exec(pattern)) !== null) {
    groups.push(match[1]);
  }
  return groups;
};

const extractTemplateTokens = (template) => {
  if (!template) {
    return [];
  }
  const tokens = [];
  const regex = /\{\{\s*([^}\s]+)\s*\}\}/g;
  let match;
  while ((match = regex.exec(template)) !== null) {
    tokens.push(match[1]);
  }
  return tokens;
};

const sanitizeName = (value) => {
  if (!value) {
    return 'Required';
  }
  const sanitized = value.replace(/[\\/:*?"<>|]/g, '_').trim();
  return sanitized || 'Required';
};

const looksLikeFilePattern = (pattern) => {
  if (!pattern) {
    return false;
  }
  return /\\\.[A-Za-z0-9]{1,6}\$?$/.test(pattern);
};

class PathStructureService extends EventEmitter {
  /**
   * @param {{ rpcService: import('./jsonRpcService').JsonRpcService }} options
   */
  constructor({ rpcService }) {
    super();
    this.rpcService = rpcService;
    this.state = {
      trackedPath: null,
      trackedFolder: null,
      currentMatch: null,
      variables: {},
      matches: [],
      selectedMatchIndex: 0,
      rawChildren: [],
      children: []
    };
  }

  /**
   * @param {import('../dtos/JsonRpcNotification').JsonRpcNotification<unknown>} payload
   */
  async handleNotification(payload) {
    if (!payload?.method) {
      return;
    }

    switch (payload.method) {
      case 'status':
        this.emit('status', {
          connected: payload?.params?.state === 'connected',
          message: payload?.params?.message || 'Watcher status updated.'
        });
        break;
      case 'watcherError':
        this.emit('status', {
          connected: true,
          message: payload?.params?.message || 'Watcher error reported.',
          errorDetails: payload?.params?.error || null
        });
        break;
      case 'watcherAborted':
        this.emit('status', {
          connected: false,
          message: payload?.params?.message || 'Watcher aborted.',
          errorDetails: payload?.params?.error || null
        });
        break;
      case 'pathChanged':
        await this.handlePathChanged(payload.params);
        break;
      default:
        break;
    }
  }

  /**
   * @param {PathChangedNotificationParams} params
   */
  async handlePathChanged(params) {
    /** @type {PathChangedNotificationParams|undefined|null} */
    const pathChanged = params;
    if (!pathChanged) {
      return;
    }

    this.state.trackedPath = pathChanged.path || null;
    this.state.variables = pathChanged.variables || {};
    const matches = Array.isArray(pathChanged.matches) ? pathChanged.matches : [];
    if (matches.length > 0) {
      this.state.matches = matches;
      this.state.selectedMatchIndex = 0;
      this.state.currentMatch = matches[0];
      const firstChildren = matches[0]?.childMatches || [];
      this.state.rawChildren = Array.isArray(firstChildren) ? firstChildren : [];
    } else {
      this.state.matches = [];
      this.state.selectedMatchIndex = 0;
      this.state.currentMatch = pathChanged.currentMatch || null;
      const immediateChildren = pathChanged.immediateChildMatches || [];
      this.state.rawChildren = Array.isArray(immediateChildren) ? immediateChildren : [];
    }
    await this.refreshValidation();
  }

  /**
   * @returns {Promise<void>}
   */
  async refreshValidation() {
    const basePath = this.state.currentMatch?.matchedValue || this.state.trackedPath;
    const trackedFolder = await this.resolveTrackedFolder(basePath);
    this.state.trackedFolder = trackedFolder;

    const entries = await this.readDirectoryEntries(trackedFolder);
    const children = this.state.rawChildren.map((child) =>
      this.buildChildState(child, entries, trackedFolder, basePath, this.state.variables)
    );

    this.state.children = children;
    this.emit('update', {
      trackedPath: this.state.trackedPath,
      trackedName: this.state.currentMatch?.name || null,
      trackedFolder: this.state.trackedFolder,
      currentFlavorText: this.buildCurrentFlavorText(),
      matches: this.state.matches,
      selectedMatchIndex: this.state.selectedMatchIndex,
      children: this.state.children
    });
  }

  /**
   * @param {number} index
   * @returns {Promise<void>}
   */
  async selectMatchIndex(index) {
    if (!Array.isArray(this.state.matches) || this.state.matches.length === 0) {
      return;
    }

    if (index < 0 || index >= this.state.matches.length) {
      return;
    }

    this.state.selectedMatchIndex = index;
    this.state.currentMatch = this.state.matches[index];
    const childMatches = this.state.currentMatch?.childMatches || [];
    this.state.rawChildren = Array.isArray(childMatches) ? childMatches : [];
    await this.refreshValidation();
  }

  /**
   * @returns {string}
   */
  buildCurrentFlavorText() {
    const template = this.state.currentMatch?.flavorTextTemplate || '';
    return renderTemplateWithVariables(template, this.state.variables);
  }

  /**
   * @returns {Promise<{ created: string[], skipped: string[], message: string }>}
   */
  async scaffoldRequiredFolders() {
    const created = [];
    const skipped = [];

    if (!this.state.trackedFolder) {
      return { created, skipped, message: 'No tracked folder available.' };
    }

    for (const child of this.state.children) {
      if (!child.isRequired || child.exists || child.isFile) {
        skipped.push(child.name);
        continue;
      }

      const folderPath = path.join(this.state.trackedFolder, sanitizeName(child.name));
      if (fs.existsSync(folderPath)) {
        skipped.push(child.name);
        continue;
      }

      try {
        fs.mkdirSync(folderPath, { recursive: true });
        created.push(folderPath);
      } catch (error) {
        skipped.push(child.name);
      }
    }

    await this.refreshValidation();

    const message = created.length
      ? `Scaffolded ${created.length} required folder${created.length === 1 ? '' : 's'}.`
      : 'No required folders needed scaffolding.';

    return { created, skipped, message };
  }

  /**
   * @param {string|null} trackedPath
   * @returns {Promise<string|null>}
   */
  async resolveTrackedFolder(trackedPath) {
    if (!trackedPath) {
      return null;
    }
    try {
      const stats = fs.statSync(trackedPath);
      if (stats.isFile()) {
        return path.dirname(trackedPath);
      }
      if (stats.isDirectory()) {
        return trackedPath;
      }
    } catch (error) {
      return trackedPath;
    }
    return trackedPath;
  }

  /**
   * @param {string|null} trackedFolder
   * @returns {Promise<DirectoryEntries>}
   */
  async readDirectoryEntries(trackedFolder) {
    if (!trackedFolder) {
      return { entries: [], error: 'No tracked folder available.' };
    }

    try {
      const items = fs.readdirSync(trackedFolder, { withFileTypes: true });
      return {
        entries: items.map((item) => ({
          name: item.name,
          fullPath: path.join(trackedFolder, item.name),
          isDirectory: item.isDirectory(),
          isFile: item.isFile()
        })),
        error: null
      };
    } catch (error) {
      return { entries: [], error: 'Unable to read tracked folder contents.' };
    }
  }

  /**
   * @param {PathMatchDto} child
   * @param {DirectoryEntries} entryInfo
   * @param {string|null} trackedFolder
   * @param {string|null} trackedPath
   * @param {Record<string, string>} variables
   * @returns {ChildStructureState}
   */
  buildChildState(child, entryInfo, trackedFolder, trackedPath, variables) {
    const name = child?.name || '';
    const pattern = child?.pattern || '';
    const flavorTextTemplate = child?.flavorTextTemplate || '';
    const isRequired = Boolean(child?.isRequired);
    const matchedValue = child?.matchedValue || '';
    const icon = child?.icon ?? null;
    const backgroundColor = child?.backgroundColor ?? null;
    const foregroundColor = child?.foregroundColor ?? null;

    const exceptions = [];

    let regex;
    try {
      regex = pattern ? new RegExp(pattern) : null;
    } catch (error) {
      exceptions.push({ severity: 'fatal', message: 'Invalid regex pattern.' });
    }

    const groupNames = extractGroupNames(pattern);
    const templateTokens = extractTemplateTokens(flavorTextTemplate);
    const missingTokens = templateTokens.filter((token) =>
      !groupNames.includes(token) && !(variables && Object.prototype.hasOwnProperty.call(variables, token))
    );
    if (missingTokens.length > 0) {
      exceptions.push({
        severity: 'warning',
        message: `Flavor text tokens not found in pattern: ${missingTokens.join(', ')}.`
      });
    }

    let matchedEntry = null;
    let matchedEntryResult = null;
    const matchingEntries = [];
    if (regex && entryInfo?.entries?.length) {
      entryInfo.entries.forEach((entry) => {
        let result = regex.exec(entry.fullPath);
        if (!result?.length) {
          result = regex.exec(normalizePath(entry.fullPath));
        }
        if (!result?.length) {
          result = regex.exec(entry.name);
        }
        if (result?.length) {
          matchingEntries.push({ entry, result });
          if (!matchedEntry) {
            matchedEntry = entry;
            matchedEntryResult = result;
          }
        }
      });
    }

    let fallbackMatch = null;
    if (!matchedEntryResult && regex && matchedValue) {
      fallbackMatch = regex.exec(matchedValue);
    }

    let lineageMatch = null;
    if (!matchedEntryResult && !fallbackMatch && regex) {
      lineageMatch = tryMatchAgainstPath(regex, trackedFolder) || tryMatchAgainstPath(regex, trackedPath);
      if (!lineageMatch) {
        const relaxedPattern = buildGroupCaptureRegex(pattern);
        if (relaxedPattern) {
          try {
            const relaxedRegex = new RegExp(relaxedPattern);
            lineageMatch = tryMatchAgainstPath(relaxedRegex, trackedFolder) || tryMatchAgainstPath(relaxedRegex, trackedPath);
          } catch (error) {
            exceptions.push({ severity: 'warning', message: 'Unable to loosen regex for group extraction.' });
          }
        }
      }
    }

    const exists = Boolean(matchedEntry);
    const isFile = matchedEntry ? matchedEntry.isFile : looksLikeFilePattern(pattern);

    if (entryInfo?.error) {
      exceptions.push({ severity: 'fatal', message: entryInfo.error });
    } else if (isRequired && !exists) {
      exceptions.push({ severity: 'error', message: 'Required path not found in tracked folder.' });
    }

    const resolvedName = name || sanitizeName(pattern);
    let displayPath = matchedEntry?.name || resolvedName;
    if (matchedEntry?.fullPath && trackedFolder) {
      const relativePath = path.relative(trackedFolder, matchedEntry.fullPath);
      displayPath = relativePath.startsWith('..') ? matchedEntry.fullPath : relativePath;
    }
    const literalPath =
      matchedEntry?.fullPath ||
      (trackedFolder && displayPath ? path.join(trackedFolder, displayPath) : null) ||
      pattern ||
      displayPath;
    const templateMatch = matchedEntryResult || fallbackMatch || lineageMatch;
    const flavorText = templateMatch?.groups
      ? renderTemplate(flavorTextTemplate, templateMatch)
      : renderTemplateWithVariables(flavorTextTemplate, variables);

    return {
      name: resolvedName,
      displayPath,
      literalPath,
      flavorText,
      pattern,
      isRequired,
      icon,
      backgroundColor,
      foregroundColor,
      matchingPaths: matchingEntries.map((item) => item.entry.fullPath),
      trackedFolder,
      exists,
      isFile,
      exceptions,
      severity: this.getMaxSeverity(exceptions)
    };
  }

  getMaxSeverity(exceptions) {
    if (!exceptions?.length) {
      return null;
    }
    return exceptions.reduce((max, exception) =>
      getSeverityRank(exception.severity) > getSeverityRank(max) ? exception.severity : max, null
    );
  }
}

module.exports = { PathStructureService };
