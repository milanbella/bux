import { callGetTotalBuxEarned, callGetAavatar64 } from './api.js';


export function setRoBloxUser(username: string) {
    let elem = document.getElementById('header-user-name');
    if (elem !== null) {
        displayAavatar();
        elem.textContent = username;
    } else {
        console.error('could not find element');
    }

    setReferralUrl(username);
}

export function setTotalBux(totalBux: number) {
    let elem = document.getElementById('header-total-bux');
    if (elem !== null) {
        elem.textContent = `(${totalBux} bux)`;
    } else {
        console.warn('could not find total bux element');
    }
}

export async function updateTotalBux() {
    let res = await callGetTotalBuxEarned();
    if (res.response) {
        setTotalBux(res.response.totalBux);
    }
}

export async function displayAavatar() {
    let elem = document.getElementById('avatar-image-container')
    if (!elem) {
        console.warn('avatar-image-container not found');
        return;
    }
    elem.innerHTML = '<i class="fa-solid fa-user">'; 
    let res = await callGetAavatar64();
    if (res.response) {
        if (res.response.url64) {
            elem.innerHTML = `<img src="${res.response.url64}"></img>`
        }
    }
}

export function setReferralUrl(username: string) {
    let referralLinkEl = document.getElementById('referral-link') as HTMLElement | null;
    if (referralLinkEl) {
        let url = `${window.location.protocol}//${window.location.host}/b/referral?code=${username}`
        referralLinkEl.innerText = url;
    }
}

