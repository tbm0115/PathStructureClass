const { EventEmitter } = require('events');
const fs = require('fs');
const path = require('path');

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
  constructor({ rpcService }) {
    super();
    this.rpcService = rpcService;
    this.state = {
      trackedPath: null,
      trackedFolder: null,
      currentMatch: null,
      variables: {},
      rawChildren: [],
      children: []
    };
  }

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
          message: payload?.params?.message || 'Watcher error reported.'
        });
        break;
      case 'watcherAborted':
        this.emit('status', {
          connected: false,
          message: payload?.params?.message || 'Watcher aborted.'
        });
        break;
      case 'pathChanged':
        await this.handlePathChanged(payload.params);
        break;
      default:
        break;
    }
  }

  async handlePathChanged(params) {
    this.state.trackedPath = params?.path || null;
    this.state.currentMatch = params?.currentMatch || null;
    this.state.variables = params?.variables || {};
    const immediateChildren =
      params?.immediateChildMatches ||
      params?.ImmediateChildMatches ||
      params?.immediateChildren ||
      params?.children;
    this.state.rawChildren = Array.isArray(immediateChildren) ? immediateChildren : [];
    await this.refreshValidation();
  }

  async refreshValidation() {
    const trackedFolder = await this.resolveTrackedFolder(this.state.trackedPath);
    this.state.trackedFolder = trackedFolder;

    const entries = await this.readDirectoryEntries(trackedFolder);
    const children = this.state.rawChildren.map((child) =>
      this.buildChildState(child, entries, trackedFolder, this.state.trackedPath, this.state.variables)
    );

    this.state.children = children;
    this.emit('update', {
      trackedPath: this.state.trackedPath,
      trackedFolder: this.state.trackedFolder,
      currentFlavorText: this.buildCurrentFlavorText(),
      children: this.state.children
    });
  }

  buildCurrentFlavorText() {
    const template =
      this.state.currentMatch?.FlavorTextTemplate ||
      this.state.currentMatch?.flavorTextTemplate ||
      '';
    return renderTemplateWithVariables(template, this.state.variables);
  }

  async scaffoldRequiredFolders() {
    const created = [];
    const skipped = [];

    if (!this.state.trackedFolder) {
      return { created, skipped, message: 'No tracked folder available.' };
    }

    for (const child of this.state.children) {
      if (!child.isRequired || child.exists || child.isFile) {
        skipped.push(child.displayName);
        continue;
      }

      const folderPath = path.join(this.state.trackedFolder, sanitizeName(child.displayName));
      if (fs.existsSync(folderPath)) {
        skipped.push(child.displayName);
        continue;
      }

      try {
        fs.mkdirSync(folderPath, { recursive: true });
        created.push(folderPath);
      } catch (error) {
        skipped.push(child.displayName);
      }
    }

    await this.refreshValidation();

    const message = created.length
      ? `Scaffolded ${created.length} required folder${created.length === 1 ? '' : 's'}.`
      : 'No required folders needed scaffolding.';

    return { created, skipped, message };
  }

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

  buildChildState(child, entryInfo, trackedFolder, trackedPath, variables) {
    const nodeName = child?.NodeName || child?.nodeName || child?.name;
    const pattern = child?.Pattern || child?.pattern || '';
    const flavorTextTemplate = child?.FlavorTextTemplate || child?.flavorTextTemplate || '';
    const isRequired = Boolean(child?.isRequired ?? child?.IsRequired);
    const matchedValue = child?.MatchedValue || child?.matchedValue || '';
    const icon = child?.Icon || child?.icon || null;
    const backgroundColor = child?.BackgroundColor || child?.backgroundColor || null;
    const foregroundColor = child?.ForegroundColor || child?.foregroundColor || null;

    const exceptions = [];

    let regex;
    try {
      regex = pattern ? new RegExp(pattern) : null;
    } catch (error) {
      exceptions.push({ severity: 'fatal', message: 'Invalid regex pattern.' });
    }

    const groupNames = extractGroupNames(pattern);
    const templateTokens = extractTemplateTokens(flavorTextTemplate);
    const missingTokens = templateTokens.filter((token) => !groupNames.includes(token));
    if (missingTokens.length > 0) {
      exceptions.push({
        severity: 'warning',
        message: `Flavor text tokens not found in pattern: ${missingTokens.join(', ')}.`
      });
    }

    let matchedEntry = null;
    let matchedEntryResult = null;
    if (regex && entryInfo?.entries?.length) {
      matchedEntry = entryInfo.entries.find((entry) => {
        matchedEntryResult = regex.exec(entry.fullPath);
        if (matchedEntryResult?.length) {
          return true;
        }
        matchedEntryResult = regex.exec(normalizePath(entry.fullPath));
        if (matchedEntryResult?.length) {
          return true;
        }
        matchedEntryResult = regex.exec(entry.name);
        return Boolean(matchedEntryResult?.length);
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

    const displayName = matchedEntry?.name || nodeName || sanitizeName(pattern);
    const templateMatch = matchedEntryResult || fallbackMatch || lineageMatch;
    const flavorText = templateMatch?.groups
      ? renderTemplate(flavorTextTemplate, templateMatch)
      : renderTemplateWithVariables(flavorTextTemplate, variables);

    return {
      displayName,
      flavorText,
      pattern,
      isRequired,
      icon,
      backgroundColor,
      foregroundColor,
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
