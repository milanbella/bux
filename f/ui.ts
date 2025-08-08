export function showMessage(message: string, type: string): void {
  const el = document.getElementById('message');
  if (!el) return;
  el.textContent = message;
  el.className = `message ${type}`.trim();
}
