document.addEventListener('DOMContentLoaded', () => {
  const listElement = document.getElementById('import-list');
  const emptyState = document.getElementById('import-empty');
  const statusElement = document.getElementById('import-status');
  const refreshButton = document.getElementById('refresh-imports');

  const setStatus = (message, isError = false) => {
    if (!statusElement) {
      return;
    }
    statusElement.textContent = message || '';
    statusElement.classList.toggle('muted', !isError);
  };

  const buildField = (labelText, value, placeholder = '') => {
    const wrapper = document.createElement('label');
    wrapper.className = 'field';

    const label = document.createElement('span');
    label.textContent = labelText;
    wrapper.appendChild(label);

    const input = document.createElement('input');
    input.type = 'text';
    input.value = value || '';
    input.placeholder = placeholder;
    wrapper.appendChild(input);

    return { wrapper, input };
  };

  const renderImports = (imports) => {
    if (!listElement) {
      return;
    }

    listElement.innerHTML = '';

    if (!imports || imports.length === 0) {
      if (emptyState) {
        emptyState.hidden = false;
      }
      return;
    }

    if (emptyState) {
      emptyState.hidden = true;
    }

    imports.forEach((importItem) => {
      const item = document.createElement('li');
      item.className = 'path-item import-item';

      const header = document.createElement('div');
      header.className = 'path-item-header';

      const pathLabel = document.createElement('span');
      pathLabel.className = 'path-item-label';
      pathLabel.textContent = importItem.path || '';
      header.appendChild(pathLabel);
      item.appendChild(header);

      const fieldStack = document.createElement('div');
      fieldStack.className = 'import-fields';

      const namespaceField = buildField('Namespace', importItem.namespace, 'Optional namespace');
      const rootField = buildField('Root path', importItem.rootPath, 'Optional root path');

      fieldStack.appendChild(namespaceField.wrapper);
      fieldStack.appendChild(rootField.wrapper);
      item.appendChild(fieldStack);

      const actions = document.createElement('div');
      actions.className = 'import-actions';

      const saveButton = document.createElement('button');
      saveButton.className = 'action-button';
      saveButton.type = 'button';
      saveButton.textContent = 'Save';

      const removeButton = document.createElement('button');
      removeButton.className = 'ghost-button';
      removeButton.type = 'button';
      removeButton.textContent = 'Remove';

      actions.appendChild(saveButton);
      actions.appendChild(removeButton);
      item.appendChild(actions);

      saveButton.addEventListener('click', async () => {
        setStatus('Saving import changes...');
        try {
          await window.pathStructure?.sendJsonRpcRequest('updateImport', {
            path: importItem.path,
            namespace: namespaceField.input.value.trim() || null,
            rootPath: rootField.input.value.trim() || null
          });
          setStatus('Import updated.');
          await loadImports();
        } catch (error) {
          setStatus('Unable to update import.', true);
          console.error(error);
        }
      });

      removeButton.addEventListener('click', async () => {
        const confirmed = window.confirm('Remove this import?');
        if (!confirmed) {
          return;
        }
        setStatus('Removing import...');
        try {
          await window.pathStructure?.sendJsonRpcRequest('removeImport', { path: importItem.path });
          setStatus('Import removed.');
          await loadImports();
        } catch (error) {
          setStatus('Unable to remove import.', true);
          console.error(error);
        }
      });

      listElement.appendChild(item);
    });
  };

  const loadImports = async () => {
    if (!window.pathStructure?.sendJsonRpcRequest) {
      setStatus('JSON-RPC service not available.', true);
      return;
    }

    setStatus('Loading imports...');
    try {
      const result = await window.pathStructure.sendJsonRpcRequest('listImports');
      renderImports(result?.imports || []);
      setStatus('');
    } catch (error) {
      setStatus('Unable to load imports.', true);
      console.error(error);
    }
  };

  if (refreshButton) {
    refreshButton.addEventListener('click', () => {
      void loadImports();
    });
  }

  void loadImports();
});
