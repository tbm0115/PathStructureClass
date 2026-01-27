document.addEventListener('DOMContentLoaded', () => {
  if (window.pathStructure?.platform) {
    document.body.dataset.platform = window.pathStructure.platform;
  }

  const statusElement = document.getElementById('connection-status');
  const statusMessage = document.getElementById('status-message');
  const trackedPathElement = document.getElementById('tracked-path');
  const trackedFlavorElement = document.getElementById('tracked-flavor');
  const trackedFolderElement = document.getElementById('tracked-folder');
  const listElement = document.getElementById('path-structure-list');
  const emptyState = document.getElementById('empty-state');
  const countElement = document.getElementById('child-count');
  const childPanel = document.getElementById('child-panel');
  const searchInput = document.getElementById('child-search');
  const addPathButton = document.getElementById('add-path');
  const modalOverlay = document.getElementById('modal-overlay');
  const addPathForm = document.getElementById('add-path-form');
  const closeModalButton = document.getElementById('close-modal');
  const cancelAddButton = document.getElementById('cancel-add');
  const toggleServiceButton = document.getElementById('toggle-service');

  let allChildren = [];
  let currentSearch = '';
  let isConnected = false;

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

    const mainRow = document.createElement('div');
    mainRow.className = 'path-item-main';

    const name = document.createElement('span');
    name.className = 'path-name';
    name.textContent = child.literalPath || child.displayName;

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
    allChildren = children;

    const isFileSelection =
      Boolean(payload?.trackedPath) &&
      Boolean(payload?.trackedFolder) &&
      payload.trackedPath !== payload.trackedFolder;

    if (childPanel) {
      childPanel.hidden = isFileSelection;
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

    renderList();
  };

  const closeModal = () => {
    if (!modalOverlay) {
      return;
    }
    modalOverlay.hidden = true;
    if (addPathForm) {
      addPathForm.reset();
    }
  };

  const openModal = () => {
    if (!modalOverlay) {
      return;
    }
    modalOverlay.hidden = false;
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
      openModal();
    });
  }

  if (closeModalButton) {
    closeModalButton.addEventListener('click', closeModal);
  }

  if (cancelAddButton) {
    cancelAddButton.addEventListener('click', closeModal);
  }

  if (modalOverlay) {
    modalOverlay.addEventListener('click', (event) => {
      if (event.target === modalOverlay) {
        closeModal();
      }
    });
  }

  if (addPathForm) {
    addPathForm.addEventListener('submit', async (event) => {
      event.preventDefault();
      const formData = new FormData(addPathForm);
      const regex = formData.get('regex');
      if (!regex) {
        return;
      }
      const payload = {
        regex: String(regex).trim(),
        name: String(formData.get('name') || '').trim(),
        flavorTextTemplate: String(formData.get('flavorTextTemplate') || '').trim(),
        isRequired: formData.get('isRequired') === 'on'
      };

      try {
        await window.pathStructure?.sendJsonRpcRequest('addPath', payload);
        updateStatus({
          connected: true,
          message: 'Path added. Reloading configuration...'
        });
        await window.pathStructure?.softReset();
        closeModal();
      } catch (error) {
        updateStatus({
          connected: isConnected,
          message: 'Unable to update configuration. Please check the configuration file.'
        });
        console.error('Add path failed:', error);
      }
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
