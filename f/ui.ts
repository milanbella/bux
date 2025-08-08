export function showMessage(severity: 'info' | 'warn' | 'error', message: string): void {

    const el = document.getElementById('message');
    if (!el) return;

    el.textContent = message;
    el.className = `message ${severity}`;
}

export function clearMessage(): void {

    const el = document.getElementById('message');
    if (!el) return;

    el.textContent = '';
    el.className = `message`;
}

export function initMessageBox(): void {
    const el = document.getElementById('message');
    if (!el) return;

    el.addEventListener('click', () => {
        clearMessage();
    });
}

initMessageBox();

