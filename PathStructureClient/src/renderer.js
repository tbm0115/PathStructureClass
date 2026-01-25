document.addEventListener('DOMContentLoaded', () => {
  const panels = document.querySelectorAll('.panel');
  panels.forEach((panel) => {
    panel.addEventListener('mouseenter', () => panel.classList.add('active'));
    panel.addEventListener('mouseleave', () => panel.classList.remove('active'));
  });
});
