document.addEventListener('DOMContentLoaded', () => {
  const addPathForm = document.getElementById('add-path-form');
  const closeWindowButton = document.getElementById('close-window');
  const cancelButton = document.getElementById('cancel-add');

  const closeWindow = () => {
    window.close();
  };

  if (closeWindowButton) {
    closeWindowButton.addEventListener('click', closeWindow);
  }

  if (cancelButton) {
    cancelButton.addEventListener('click', closeWindow);
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
        await window.pathStructure?.softReset();
        await window.pathStructure?.notifyStatus({
          connected: true,
          message: 'Path added. Reloading configuration...'
        });
        closeWindow();
      } catch (error) {
        await window.pathStructure?.notifyStatus({
          connected: true,
          message: 'Unable to update configuration. Please check the configuration file.',
          errorDetails: error?.message || error
        });
        console.error('Add path failed:', error);
      }
    });
  }
});
