import { callGetData, callPostData, Response } from './api.js';

interface ClickResponse {
    buxAmount: number;
    clicksCount: number;
}

interface GameState {
    clicksCount: number;
}

const clickButton = document.getElementById('click-button');
if (clickButton == null) {
	throw new Error("missing click button");
}

const resultContainer = document.getElementById('result-container');
if (resultContainer == null) {
	throw new Error("missing result container");
}

export async function callClick(): Promise<Response<ClickResponse>> {
    let response = await callPostData<ClickResponse>('b/api/click-game/click', '{}');
    return response;
}

export async function callGetGameState(): Promise<Response<GameState>> {
    let response = await callGetData<GameState>('b/api/click-game/game-state');
    return response;
}


function hideCountButton() {
	(clickButton as HTMLElement).style.visibility = 'hidden';
}

function showCountButton(count: number) {
	(clickButton as HTMLElement).style.visibility = 'visible';
	if (count > 1) {
		(clickButton as HTMLElement).textContent = `click me ${count} times to earn a bux`;
	} else {
		(clickButton as HTMLElement).textContent = `click me ${count} time to earn a bux`;
	}
}

function hideResultContainer() {
	(resultContainer as HTMLElement).style.visibility = 'hidden';
}

function showResultContainer(buxEarned: number) {
	(resultContainer as HTMLElement).style.visibility = 'visible';
    let textElem = document.getElementById('bux-earned-text') as HTMLElement;
    if (textElem) {
        textElem.textContent = `Congratulation! You just earned ${buxEarned} bux!`;
    } else {
        throw new Error('missing element: bux-earned-text'); 
    }
}


hideResultContainer();
//showResultContainer(5); //@@@@@@@@@@@@@@@@@@@@@@

hideCountButton();
callGetGameState().then((res) => {
    console.log('@@@@@@@@@@@@@@@@@@@@@@@@@@@@ cp 100: callGetGameState');
    console.dir(res.response); //@@@@@@@@@@@@@@@@@@@@@@
	if (res.response) {
		showCountButton(res.response.clicksCount);
	}
})

if (clickButton == null) {
	throw new Error("missing click button");
}
clickButton.addEventListener("click", async () => {
	try {
		hideCountButton();
        hideResultContainer();
		const res = await callClick();
		console.log('@@@@@@@@@@@@@@@@@@@@@@@@@@@@ cp 200: callClick');
		console.dir(res.response); // log the response from backend
		if (res.response) {
			showCountButton(res.response.clicksCount);
            if (res.response.buxAmount > 0) {
                showResultContainer(res.response.buxAmount)
            }
		}
			
	} catch (err) {
		console.error("Error calling click-game API:", err);
	}
});
