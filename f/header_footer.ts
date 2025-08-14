export function setRoBloxUser(username: string) {
    let elem = document.getElementById("header-user-name");
    if (elem !== null) {
        elem.textContent = username;
    } else {
        console.warn("could not find element");
    }
}
