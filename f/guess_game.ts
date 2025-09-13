import {callGetData, callPostData, Response} from './src/api.js';
import { updateTotalBux } from './src/header_footer.js';


interface ClickRequest {
    guessNumber: number;
}


interface ClickResponse {
    buxAmount: number;
    isMatch: boolean
}

interface GameState {
    min: number;
    max: number;
}

const formTitle = document.getElementById('form-title');
if (formTitle == null) {
    throw new Error("missing form title");
}

const guessButton = document.getElementById('guess-button');
if (guessButton == null) {
    throw new Error("missing guess button");
}

const resultContainer = document.getElementById('result-container');
if (resultContainer == null) {
    throw new Error("missing result container");
}

export async function callGuess(guessNumber: number): Promise<Response<ClickResponse>> {
    let request: ClickRequest = {
        guessNumber: guessNumber
    }
    let data = JSON.stringify(request)
    let response = await callPostData<ClickResponse>('b/api/guess-game/guess', data);
    return response;
}

export async function callGetGameState(): Promise<Response<GameState>> {
    let response = await callGetData<GameState>('b/api/guess-game/game-state');
    return response;
}

function hideGuessButton() {
    (guessButton as HTMLElement).style.visibility = 'hidden';
}

function showGuessButton() {
    (guessButton as HTMLElement).style.visibility = 'visible';
}

function hideResultContainer() {
    (resultContainer as HTMLElement).style.visibility = 'hidden';
}

function showResultContainer(isMatch: boolean, buxEarned: number) {
    (resultContainer as HTMLElement).style.visibility = 'visible';
    let textElem = document.getElementById('bux-earned-text') as HTMLElement;
    if (textElem) {
        if (isMatch) {
            textElem.textContent = `Congratulation your guess is correct! You just earned ${buxEarned} bux!`;
        } else {
            textElem.textContent = `Wrong guess!`;
        }
    } else {
        throw new Error('missing element: bux-earned-text');
    }
}

hideResultContainer();

hideGuessButton();
callGetGameState().then((res) => {
    console.log('@@@@@@@@@@@@@@@@@@@@@@@@@@@@ cp 100: callGetGameState');
    console.dir(res.response); //@@@@@@@@@@@@@@@@@@@@@@
    if (res.response) {
        formTitle.textContent = `Guess the number between ${res.response.min} -  ${res.response.max} inclusive.`

        const guessInput = document.querySelector<HTMLInputElement>('input[name="guess-number"]');
        if (guessInput) {
            guessInput.min = String(res.response.min);
            guessInput.max = String(res.response.max);
        } else {
            throw new Error("missing guess input");
        }

        showGuessButton();
    } else {
        throw new Error("no response");
    }
})

if (guessButton == null) {
    throw new Error("missing guess button");
}
guessButton.addEventListener("click", async () => {
    try {
        const form = document.getElementById('guess-form') as HTMLFormElement | null;
        if (!form) {
            throw new Error('missing guess form');
        }
        const guessNumber = Number((form['guess-number'] as HTMLInputElement).value);

        hideGuessButton();
        hideResultContainer();
        const res = await callGuess(guessNumber);
        console.log('@@@@@@@@@@@@@@@@@@@@@@@@@@@@ cp 200: callGuess');
        console.dir(res.response); // log the response from backend
        if (res.response) {
            showGuessButton();
            showResultContainer(res.response.isMatch, res.response.buxAmount)
            if (res.response.buxAmount > 0) {
                updateTotalBux();
            }
        } else {
            throw new Error('mo response');
        }

    } catch (err) {
        console.error("Error calling click-game API:", err);
    }
});
