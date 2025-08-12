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
        const username = (form.username as HTMLInputElement).value.trim();

        clearMessage();

        let request: BrowserRegisterRequest = {
            username: username,
        };
        const result = await callBrowserRegister(request);
        console.dir(result); //@@@@@@@@@@@@@@@@@@@@@@@@@
        if (result.response === null) {
            showMessage('error', result.message);
        }
    });
});
