import { callBrowserRegister, BrowserRegisterRequest } from './api.js';
import { showMessage, clearMessage } from './ui.js';

document.addEventListener('DOMContentLoaded', () => {

    const form = document.getElementById('register-form') as HTMLFormElement | null;

    if (!form) {
        console.error("no form")
        return;
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const email = (form.email as HTMLInputElement).value.trim();
        const password = (form.password as HTMLInputElement).value;
        const confirmPassword = (form['confirm-password'] as HTMLInputElement).value;

        clearMessage();

        if (password !== confirmPassword) {
            return showMessage('error', 'Passwords do not match.');
        }

        let request: BrowserRegisterRequest = {
            username: email,
            email: email,
            password: password,
            passwordVerify: confirmPassword,
        };
        const result = await callBrowserRegister(request);
        console.dir(result); //@@@@@@@@@@@@@@@@@@@@@@@@@
        if (result.response === null) {
            showMessage('error', result.message);
        }
    });
});
