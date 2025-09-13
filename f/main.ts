import { callPostData, callGetUser } from './src/api.js'
import { displayAavatar, setRoBloxUser, updateTotalBux } from './src/header_footer.js'

const FILE = "main.ts"

async function displayCurrentUser() {
    let result = await callGetUser()
    if (result.response !== null) {
        setRoBloxUser(result.response.username || '');
    }
}

async function handleDiscordClick() 
{
    const FUNCTION = "handleDiscordClick()"
    let discordWasClicked: boolean = false;

    async function callDiscordClick(event: MouseEvent): Promise<void> {
         event.preventDefault(); // stop default "#" navigation

        if (!discordWasClicked) {
            let response = await callPostData('b/api1/discord-click', '{}');
            console.log(`${FILE}:${FUNCTION}: response:`);
            console.dir(response);
        } else {
            console.log(`${FILE}:${FUNCTION}: noop:`);
        }
        window.location.href = "https://discord.gg/ZFCMVTtgcJ";
    }

    let discordBtn = document.getElementById('top-buttons-discord-btn');
    if (discordBtn) {
        discordBtn.addEventListener('click', callDiscordClick)
    } else {
        console.error(`${FILE}:${FUNCTION}: no element: discord-btn:`);
    }
}

document.addEventListener("DOMContentLoaded", function () {
    displayCurrentUser();
    updateTotalBux();
    displayAavatar();
    handleDiscordClick();
});
