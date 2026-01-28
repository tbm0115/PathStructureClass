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

  const renderChild = (child) => {
    const item = document.createElement('li');
    item.className = 'path-item';

    const pathRow = document.createElement('div');
    pathRow.className = 'path-item-header';

    const pathLabel = document.createElement('span');
    pathLabel.className = 'path-item-label';
    pathLabel.textContent = child.literalPath || '';
    pathRow.appendChild(pathLabel);

    if (child.isRequired) {
      const badge = document.createElement('span');
      badge.className = 'required-badge';
      badge.textContent = 'Required';
      pathRow.appendChild(badge);
    }

    const severity = getMaxSeverity(child.exceptions);
    if (severity) {
      const icon = document.createElement('span');
      icon.className = `validation-icon severity-${severity}`;
      icon.textContent = '●';
      icon.title = child.exceptions.map((exception) => exception.message).join('\n');
      pathRow.appendChild(icon);
    }

    item.appendChild(pathRow);

    const mainRow = document.createElement('div');
    mainRow.className = 'path-item-main';

    const name = document.createElement('strong');
    name.className = 'path-name';
    name.textContent = child.displayName || '';

    mainRow.appendChild(name);

    item.appendChild(mainRow);

    if (child.flavorText) {
      const flavor = document.createElement('div');
      flavor.className = 'path-flavor';
      flavor.textContent = child.flavorText;
      item.appendChild(flavor);
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
      return (
        child.literalPath?.toLowerCase().includes(query) ||
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
