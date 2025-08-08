import { postData } from './api.js';
import { showMessage } from './ui.js';

document.addEventListener('DOMContentLoaded', () => {
  const form = document.getElementById('register-form') as HTMLFormElement | null;
  if (!form) return;

  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    const email = (form.email as HTMLInputElement).value.trim();
    const password = (form.password as HTMLInputElement).value;
    const confirmPassword = (form['confirm-password'] as HTMLInputElement).value;

    showMessage('', '');

    if (password !== confirmPassword) {
      return showMessage('Passwords do not match.', 'error');
    }

    try {
      const result = await postData('/api/register', { email, password });
      if (result && !(result as any).error) {
        showMessage((result as any).message || 'Registration successful!', 'success');
      } else {
        showMessage((result as any).error || 'Registration failed. Please try again.', 'error');
      }
    } catch {
      showMessage('Network error. Please try again later.', 'error');
    }
  });
});
