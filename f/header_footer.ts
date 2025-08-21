import { callGetTotalBuxEarned } from './api.js';

export function setRoBloxUser(username: string) {
    let elem = document.getElementById("header-user-name");
    if (elem !== null) {
        elem.textContent = username;
    } else {
        console.warn("could not find element");
    }
}

export function setTotalBux(totalBux: number) {
    let elem = document.getElementById("header-total-bux");
    if (elem !== null) {
        elem.textContent = String(totalBux);
    } else {
        console.warn("could not find total bux element");
    }
}

export async function updateTotalBux() {
    let res = await callGetTotalBuxEarned();
    if (res.response) {
        setTotalBux(res.response.totalBux);
    }
}
