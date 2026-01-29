document.addEventListener('DOMContentLoaded', () => {
  if (window.pathStructure?.platform) {
    document.body.dataset.platform = window.pathStructure.platform;
  }

  const statusElement = document.getElementById('connection-status');
  const statusMessage = document.getElementById('status-message');
  const trackedNameElement = document.getElementById('tracked-name');
  const trackedPathElement = document.getElementById('tracked-path');
  const trackedFlavorElement = document.getElementById('tracked-flavor');
  const trackedFolderElement = document.getElementById('tracked-folder');
  const trackedFolderGroup = document.getElementById('tracked-folder-group');
  const matchBadge = document.getElementById('match-badge');
  const matchList = document.getElementById('match-list');
  const listElement = document.getElementById('path-structure-list');
  const emptyState = document.getElementById('empty-state');
  const countElement = document.getElementById('child-count');
  const childPanel = document.getElementById('child-panel');
  const searchInput = document.getElementById('child-search');
  const addPathButton = document.getElementById('add-path');
  const toggleServiceButton = document.getElementById('toggle-service');
  const childItemTemplate = document.getElementById('child-item-template');

  let allChildren = [];
  let currentSearch = '';
  let isConnected = false;
  let matches = [];
  let selectedMatchIndex = 0;
  let isMatchListVisible = false;

  const severityOrder = {
    warning: 1,
    error: 2,
    fatal: 3
  };

  const updateStatus = (status) => {
    if (!statusElement || !statusMessage) {
      return;
    }
    isConnected = Boolean(status.connected);
    statusElement.textContent = isConnected ? 'Connected' : 'Disconnected';
    statusElement.dataset.connected = String(isConnected);
    statusMessage.textContent = status.message || 'Watcher status updated.';
    if (toggleServiceButton) {
      toggleServiceButton.textContent = isConnected ? 'Stop service' : 'Start service';
    }
    if (status.errorDetails) {
      console.error(status.errorDetails);
    }
  };

  const getMaxSeverity = (exceptions) => {
    if (!exceptions || exceptions.length === 0) {
      return null;
    }
    return exceptions.reduce((max, exception) => {
      if (!max) {
        return exception.severity;
      }
      return severityOrder[exception.severity] > severityOrder[max] ? exception.severity : max;
    }, null);
  };

  const getTemplateElement = (template) => {
    if (!template || !(template instanceof HTMLTemplateElement)) {
      return null;
    }
    return template.content.firstElementChild?.cloneNode(true) || null;
  };

  const getField = (root, fieldName) => root.querySelector(`[data-field="${fieldName}"]`);

  const setFieldText = (root, fieldName, value, options = {}) => {
    const field = getField(root, fieldName);
    if (!field) {
      return null;
    }
    field.textContent = value;
    if (options.hideWhenEmpty) {
      field.hidden = !value;
    }
    return field;
  };

  const getChildMatchListRenderer = ({ matchList, matchingPaths, trackedFolder }) => {
    if (!matchList) {
      return null;
    }

    const trimTrackedFolder = (value) => {
      if (!value || !trackedFolder) {
        return value || '';
      }
      const normalizedFolder = trackedFolder.endsWith('\\') || trackedFolder.endsWith('/')
        ? trackedFolder
        : `${trackedFolder}${value.includes('/') ? '/' : '\\'}`;
      return value.startsWith(normalizedFolder) ? value.slice(normalizedFolder.length) : value;
    };

    const pageSize = 5;
    let page = 0;

    const renderMatchList = () => {
      matchList.innerHTML = '';
      const start = page * pageSize;
      const end = start + pageSize;
      const slice = matchingPaths.slice(start, end);

      const list = document.createElement('ul');
      list.className = 'child-match-paths';

      slice.forEach((matchPath) => {
        const listItem = document.createElement('li');
        listItem.textContent = trimTrackedFolder(matchPath);
        list.appendChild(listItem);
      });

      matchList.appendChild(list);

      if (matchingPaths.length > pageSize) {
        const controls = document.createElement('div');
        controls.className = 'child-match-controls';

        const prev = document.createElement('button');
        prev.type = 'button';
        prev.className = 'ghost-button';
        prev.textContent = 'Prev';
        prev.disabled = page === 0;
        prev.addEventListener('click', () => {
          if (page > 0) {
            page -= 1;
            renderMatchList();
          }
        });

        const next = document.createElement('button');
        next.type = 'button';
        next.className = 'ghost-button';
        next.textContent = 'Next';
        next.disabled = end >= matchingPaths.length;
        next.addEventListener('click', () => {
          if (end < matchingPaths.length) {
            page += 1;
            renderMatchList();
          }
        });

        const count = document.createElement('span');
        count.className = 'child-match-count';
        count.textContent = `${start + 1}-${Math.min(end, matchingPaths.length)} of ${matchingPaths.length}`;

        controls.appendChild(prev);
        controls.appendChild(count);
        controls.appendChild(next);
        matchList.appendChild(controls);
      }
    };

    return renderMatchList;
  };

  const getChildLabelDetails = (child) => {
    const hasMultipleMatches = Array.isArray(child.matchingPaths) && child.matchingPaths.length > 1;

    /*
      Label selection rules for the child structure name:
      1) When there are multiple matches we want a concise label. Prefer `displayName` first.
      2) If `displayName` is missing, fall back to `flavorText` (so the entry is not blank).
      3) If both are missing but a pattern exists, show the pattern in a <code> tag.
      4) When there is only a single match, only `displayName` is shown to keep labels consistent.
      5) If we used `flavorText` as the label, suppress the separate flavor row below.
    */

    if (hasMultipleMatches) {
      if (child.displayName) {
        return { label: child.displayName, isFlavorLabel: false, isPatternLabel: false };
      }
      if (child.flavorText) {
        return { label: child.flavorText, isFlavorLabel: true, isPatternLabel: false };
      }
      if (child.pattern) {
        return { label: child.pattern, isFlavorLabel: false, isPatternLabel: true };
      }
    }

    return { label: child.displayName || '', isFlavorLabel: false, isPatternLabel: false };
  };

  const renderChild = (child) => {
    const item = getTemplateElement(childItemTemplate);
    if (!item) {
      return;
    }

    setFieldText(item, 'displayPath', child.displayPath || child.literalPath || '');

    const requiredBadge = getField(item, 'requiredBadge');
    if (requiredBadge) {
      requiredBadge.hidden = !child.isRequired;
    }

    const severity = getMaxSeverity(child.exceptions);
    const severityIcon = getField(item, 'severityIcon');
    if (severityIcon) {
      if (severity) {
        severityIcon.className = `validation-icon severity-${severity}`;
        severityIcon.textContent = '●';
        severityIcon.title = child.exceptions.map((exception) => exception.message).join('\n');
        severityIcon.hidden = false;
      } else {
        severityIcon.hidden = true;
      }
    }

    const matchList = getField(item, 'matchList');
    const matchCountBadge = getField(item, 'matchCountBadge');
    const hasMultipleMatches = Array.isArray(child.matchingPaths) && child.matchingPaths.length > 1;

    if (matchCountBadge) {
      matchCountBadge.hidden = !hasMultipleMatches;
    }

    if (hasMultipleMatches && matchCountBadge && matchList) {
      matchCountBadge.textContent = `+${child.matchingPaths.length - 1}`;
      const renderMatchList = getChildMatchListRenderer({
        matchList,
        matchingPaths: child.matchingPaths,
        trackedFolder: child.trackedFolder
      });

      matchCountBadge.addEventListener('click', () => {
        matchList.classList.toggle('hidden');
        if (!matchList.classList.contains('hidden') && renderMatchList) {
          renderMatchList();
        }
      });
    }

    const { label, isFlavorLabel, isPatternLabel } = getChildLabelDetails(child);
    const nameField = getField(item, 'displayName');
    if (nameField) {
      if (isPatternLabel) {
        nameField.textContent = '';
        const code = document.createElement('code');
        code.textContent = label;
        nameField.appendChild(code);
      } else {
        nameField.textContent = label;
      }
    }

    const flavorField = setFieldText(item, 'flavorText', child.flavorText || '', {
      hideWhenEmpty: true
    });
    if (flavorField && isFlavorLabel) {
      flavorField.hidden = true;
    }

    listElement.appendChild(item);
  };

  const renderList = () => {
    if (!listElement) {
      return;
    }

    listElement.innerHTML = '';
    const filtered = allChildren.filter((child) => {
      if (!currentSearch) {
        return true;
      }
      const query = currentSearch.toLowerCase();
      const matchPaths = Array.isArray(child.matchingPaths) ? child.matchingPaths.join(' ') : '';
      return (
        child.literalPath?.toLowerCase().includes(query) ||
        matchPaths.toLowerCase().includes(query) ||
        child.displayName?.toLowerCase().includes(query) ||
        child.flavorText?.toLowerCase().includes(query)
      );
    });

    if (countElement) {
      countElement.textContent = `${filtered.length} item${filtered.length === 1 ? '' : 's'}`;
    }

    if (filtered.length === 0) {
      if (emptyState) {
        emptyState.hidden = false;
      }
      return;
    }

    if (emptyState) {
      emptyState.hidden = true;
    }

    filtered.forEach(renderChild);
  };

  const updateList = (payload) => {
    if (!listElement) {
      return;
    }

    const children = payload?.children || [];
    matches = Array.isArray(payload?.matches) ? payload.matches : [];
    selectedMatchIndex = Number.isInteger(payload?.selectedMatchIndex) ? payload.selectedMatchIndex : 0;
    allChildren = children;

    const isFileSelection =
      Boolean(payload?.trackedPath) &&
      Boolean(payload?.trackedFolder) &&
      payload.trackedPath !== payload.trackedFolder;

    if (childPanel) {
      childPanel.hidden = isFileSelection;
    }

    if (trackedNameElement) {
      trackedNameElement.textContent = payload?.trackedName || '';
    }

    if (trackedPathElement) {
      trackedPathElement.textContent = payload?.trackedPath || 'No path selected.';
    }

    if (trackedFlavorElement) {
      trackedFlavorElement.textContent = payload?.currentFlavorText || '';
      trackedFlavorElement.hidden = !payload?.currentFlavorText;
    }

    if (trackedFolderElement) {
      trackedFolderElement.textContent = payload?.trackedFolder || 'Awaiting Explorer selection.';
    }

    if (trackedFolderGroup) {
      trackedFolderGroup.hidden =
        !payload?.trackedFolder ||
        (payload?.trackedPath && payload?.trackedFolder && payload.trackedPath === payload.trackedFolder);
    }

    renderList();
    renderMatchBadge();
    renderMatchList();
  };

  const getMatchLabel = (match) => {
    if (!match) {
      return '';
    }
    return match.matchedValue || match.MatchedValue || match.pattern || match.Pattern || match.nodeName || match.NodeName || '';
  };

  const renderMatchBadge = () => {
    if (!matchBadge) {
      return;
    }
    if (!matches || matches.length <= 1) {
      matchBadge.classList.add('hidden');
      matchBadge.textContent = '';
      if (matchList) {
        matchList.classList.add('hidden');
      }
      isMatchListVisible = false;
      return;
    }
    const remaining = matches.length - 1;
    matchBadge.textContent = `${remaining} more match${remaining === 1 ? '' : 'es'}`;
    matchBadge.classList.remove('hidden');
  };

  const renderMatchList = () => {
    if (!matchList) {
      return;
    }
    if (!matches || matches.length <= 1 || !isMatchListVisible) {
      matchList.classList.add('hidden');
      matchList.innerHTML = '';
      return;
    }

    matchList.classList.remove('hidden');
    matchList.innerHTML = '';
    matches.forEach((match, index) => {
      const item = document.createElement('li');
      item.className = 'match-item';
      if (index === selectedMatchIndex) {
        item.classList.add('selected');
      }

      const label = document.createElement('span');
      label.textContent = getMatchLabel(match);
      item.appendChild(label);

      item.addEventListener('click', () => {
        if (index === selectedMatchIndex) {
          return;
        }
        window.pathStructure?.selectMatchIndex(index);
      });

      matchList.appendChild(item);
    });
  };

  window.pathStructure?.onStatus((status) => {
    updateStatus(status);
  });

  window.pathStructure?.onPathUpdate((payload) => {
    updateList(payload);
  });

  if (searchInput) {
    searchInput.addEventListener('input', (event) => {
      currentSearch = event.target.value.trim();
      renderList();
    });
  }

  if (addPathButton) {
    addPathButton.addEventListener('click', () => {
      window.pathStructure?.openAddPathWindow();
    });
  }

  if (matchBadge) {
    matchBadge.addEventListener('click', () => {
      isMatchListVisible = !isMatchListVisible;
      renderMatchList();
    });
  }

  if (toggleServiceButton) {
    toggleServiceButton.addEventListener('click', async () => {
      if (isConnected) {
        await window.pathStructure?.stopService();
      } else {
        await window.pathStructure?.startService();
      }
    });
  }

});
