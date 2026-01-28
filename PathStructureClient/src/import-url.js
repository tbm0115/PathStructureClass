document.addEventListener('DOMContentLoaded', () => {
  const form = document.getElementById('import-url-form');
  const closeButton = document.getElementById('close-window');
  const cancelButton = document.getElementById('cancel-import');

  const closeWindow = () => {
    window.close();
  };

  if (closeButton) {
    closeButton.addEventListener('click', closeWindow);
  }

  if (cancelButton) {
    cancelButton.addEventListener('click', closeWindow);
  }

  if (form) {
    form.addEventListener('submit', async (event) => {
      event.preventDefault();
      const formData = new FormData(form);
      const url = String(formData.get('url') || '').trim();
      if (!url) {
        return;
      }

      try {
        await window.pathStructure?.importUrl(url);
        closeWindow();
      } catch (error) {
        await window.pathStructure?.notifyStatus({
          connected: true,
          message: 'Unable to import configuration.',
          errorDetails: error?.message || error
        });
        console.error('Import URL failed:', error);
      }
    });
  }
});
