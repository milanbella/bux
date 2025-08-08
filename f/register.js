import { postData } from './api.js';
import { showMessage } from './ui.js';
document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('register-form');
    if (!form)
        return;
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const email = form.email.value.trim();
        const password = form.password.value;
        const confirmPassword = form['confirm-password'].value;
        showMessage('', '');
        if (password !== confirmPassword) {
            return showMessage('Passwords do not match.', 'error');
        }
        try {
            const result = await postData('/api/register', { email, password });
            if (result && !result.error) {
                showMessage(result.message || 'Registration successful!', 'success');
            }
            else {
                showMessage(result.error || 'Registration failed. Please try again.', 'error');
            }
        }
        catch {
            showMessage('Network error. Please try again later.', 'error');
        }
    });
});
//# sourceMappingURL=register.js.map