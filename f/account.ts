import {callBrowserRegister, BrowserRegisterRequest, callGetReferralsCount} from './src/api.js';
import {showMessage, clearMessage} from './src/ui.js';
import {setRoBloxUser, updateTotalBux } from './src/header_footer.js';


document.addEventListener('DOMContentLoaded', async () => {

    const form = document.getElementById('register-form') as HTMLFormElement | null;
    const referralsCountText = document.getElementById('referrals-count-text') as HTMLButtonElement | null;

    if (!form) {
        console.error('no element: register-form');
        return;
    }

    if (!referralsCountText) {
        console.error('no element:  referrals-count-text');
        return;
    }

    async function updateReferralCount() {
        if (!referralsCountText) {
            console.error('no element:  referrals-count-text');
            return;
        }
        let referralCountResponse = await  callGetReferralsCount()
        if (referralCountResponse.response) {
            referralsCountText.innerText = `referrals count: ${referralCountResponse.response.referralsCount}`
        } else {
            referralsCountText.innerText = `referrals count:`
        }
    }

    updateReferralCount();

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const username = (form.username as HTMLInputElement).value.trim();
        const captcha = form['h-captcha-response'].value || '';
        if (captcha === '') {
            console.warn("empty cpatcha");
            return;
        }

        const params = new URLSearchParams(window.location.search);
        const referralCode = params.get("referral_code") || '';

        clearMessage();


        let request: BrowserRegisterRequest = {
            username: username,
            captcha: captcha,
            referralCode: referralCode 
        };
        const result = await callBrowserRegister(request);
        if (referralCode) {
            history.replaceState({}, "", "/account.html");
        }
        console.dir(result); //@@@@@@@@@@@@@@@@@@@@@@@@@
        if (result.response === null) {
            showMessage('error', result.message);
        } else {
            setRoBloxUser(result.response.username);
            updateTotalBux();
            showMessage('info', `Your roblox name is now: ${result.response.username}`)
            document.getElementById('register-form')?.remove();
            updateReferralCount();
        }
    });

});


function handleReferral() {
    const referralCopyBtn = document.getElementById('referral-copy-btn') as HTMLButtonElement | null;

    if (!referralCopyBtn) {
        console.error('missing: referral-copy-btn');
        return;
    }


    referralCopyBtn.addEventListener("click", async () => {
        const referralLinkEl = document.getElementById('referral-link') as HTMLElement | null;
        if (!referralLinkEl) {
            console.error('missing: referral-link');
            return;
        }
        const referralLink: string = referralLinkEl.innerText;

        try {
            await navigator.clipboard.writeText(referralLink);
            referralCopyBtn.textContent = "Copied!";
            setTimeout(() => {
                referralCopyBtn.textContent = "Copy";
            }, 2000);
        } catch (err) {
            alert("Failed to copy: " + (err instanceof Error ? err.message : String(err)));
        }
    });
}
handleReferral();
