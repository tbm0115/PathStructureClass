document.addEventListener('DOMContentLoaded', () => {
  if (window.pathStructure?.platform) {
    document.body.dataset.platform = window.pathStructure.platform;
  }

  const statusElement = document.getElementById('connection-status');
  const statusMessage = document.getElementById('status-message');
  const trackedPathElement = document.getElementById('tracked-path');
  const trackedFolderElement = document.getElementById('tracked-folder');
  const listElement = document.getElementById('path-structure-list');
  const emptyState = document.getElementById('empty-state');
  const countElement = document.getElementById('child-count');

  const severityOrder = {
    warning: 1,
    error: 2,
    fatal: 3
  };

  const updateStatus = (status) => {
    if (!statusElement || !statusMessage) {
      return;
    }
    statusElement.textContent = status.connected ? 'Connected' : 'Disconnected';
    statusElement.dataset.connected = String(Boolean(status.connected));
    statusMessage.textContent = status.message || 'Watcher status updated.';
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

    const mainRow = document.createElement('div');
    mainRow.className = 'path-item-main';

    const name = document.createElement('span');
    name.className = 'path-name';
    name.textContent = child.displayName;

    if (child.isRequired) {
      const badge = document.createElement('span');
      badge.className = 'required-badge';
      badge.textContent = 'Required';
      name.appendChild(badge);
    }

    mainRow.appendChild(name);

    const severity = getMaxSeverity(child.exceptions);
    if (severity) {
      const icon = document.createElement('span');
      icon.className = `validation-icon severity-${severity}`;
      icon.textContent = '●';
      icon.title = child.exceptions.map((exception) => exception.message).join('\n');
      mainRow.appendChild(icon);
    }

    item.appendChild(mainRow);

    if (child.flavorText) {
      const flavor = document.createElement('div');
      flavor.className = 'path-flavor';
      flavor.textContent = child.flavorText;
      item.appendChild(flavor);
    }

    listElement.appendChild(item);
  };

  const updateList = (payload) => {
    if (!listElement) {
      return;
    }

    listElement.innerHTML = '';

    const children = payload?.children || [];
    if (countElement) {
      countElement.textContent = `${children.length} item${children.length === 1 ? '' : 's'}`;
    }

    if (trackedPathElement) {
      trackedPathElement.textContent = payload?.trackedPath || 'No path selected.';
    }

    if (trackedFolderElement) {
      trackedFolderElement.textContent = payload?.trackedFolder || 'Awaiting Explorer selection.';
    }

    if (children.length === 0) {
      if (emptyState) {
        emptyState.hidden = false;
      }
      return;
    }

    if (emptyState) {
      emptyState.hidden = true;
    }

    children.forEach(renderChild);
  };

  window.pathStructure?.onStatus((status) => {
    updateStatus(status);
  });

  window.pathStructure?.onPathUpdate((payload) => {
    updateList(payload);
  });
});
