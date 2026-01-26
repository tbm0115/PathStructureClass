document.addEventListener('DOMContentLoaded', () => {
  const panels = document.querySelectorAll('.panel');
  panels.forEach((panel) => {
    panel.addEventListener('mouseenter', () => panel.classList.add('active'));
    panel.addEventListener('mouseleave', () => panel.classList.remove('active'));
  });

  if (window.pathStructure?.platform) {
    document.body.dataset.platform = window.pathStructure.platform;
  }

  const statusElement = document.getElementById('watcher-status');
  const eventsElement = document.getElementById('watcher-events');
  const maxEvents = 6;

  const updateStatus = (message, connected) => {
    if (!statusElement) {
      return;
    }
    statusElement.textContent = message;
    statusElement.dataset.connected = String(Boolean(connected));
  };

  const addEvent = (payload) => {
    if (!eventsElement) {
      return;
    }
    const item = document.createElement('li');
    const time = payload.timestamp ? new Date(payload.timestamp).toLocaleTimeString() : 'now';
    item.textContent = `[${time}] ${payload.message || payload.path || 'Explorer update received.'}`;
    eventsElement.prepend(item);

    while (eventsElement.children.length > maxEvents) {
      eventsElement.removeChild(eventsElement.lastElementChild);
    }
  };

  window.pathStructure?.onWatcherStatus((status) => {
    updateStatus(status.message || 'Watcher status updated.', status.connected);
  });

  window.pathStructure?.onWatcherEvent((payload) => {
    if (payload.type === 'status') {
      updateStatus(payload.message, payload.connected ?? true);
      return;
    }
    addEvent(payload);
  });
});
