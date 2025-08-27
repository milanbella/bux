import { callGetUser } from './api.js'
import { displayAavatar, setRoBloxUser, updateTotalBux } from './header_footer.js'

async function displayCurrentUser() {
    let result = await callGetUser()
    if (result.response !== null) {
        setRoBloxUser(result.response.username || '');
    }
}

document.addEventListener("DOMContentLoaded", function () {
    displayCurrentUser();
    updateTotalBux();
    displayAavatar();
});
