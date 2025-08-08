export function showMessage(message, type) {
    const el = document.getElementById('message');
    if (!el)
        return;
    el.textContent = message;
    el.className = `message ${type}`.trim();
}
//# sourceMappingURL=ui.js.map