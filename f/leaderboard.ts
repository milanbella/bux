import { callGetData } from './src/api.js';

interface LeaderBoardLine {
    username: string;
    buxAmount: number;
    avatarUrl64: string;
}

interface GetLeaderBoardResponse {
    lines: LeaderBoardLine[];
    myPlace: number;
}

async function loadLeaderboard(): Promise<void> {
    const result = await callGetData<GetLeaderBoardResponse>('b/api/leaderboard/lines');
    const tbody = document.getElementById('leaderboard-body');
    const myPlaceElem = document.getElementById('my-place');

    if (!tbody || !myPlaceElem) {
        console.error('missing leaderboard elements');
        return;
    }

    tbody.textContent = '';

    if (result.response) {
        result.response.lines.forEach((line, index) => {
            const tr = document.createElement('tr');

            const posTd = document.createElement('td');
            posTd.textContent = String(index + 1);
            tr.appendChild(posTd);

            const userTd = document.createElement('td');
            userTd.textContent = line.username;
            tr.appendChild(userTd);

            const avatarTd = document.createElement('td');
            if (line.avatarUrl64) {
                avatarTd.innerHTML=`<img src="${line.avatarUrl64}"></img>`
            }
            tr.appendChild(avatarTd)

            /*
            const buxTd = document.createElement('td');
            //buxTd.textContent = String(line.buxAmount) + ' R$';
            buxTd.textContent = Math.floor(line.buxAmount) + ' R$';
            tr.appendChild(buxTd);
            */

            const buxTd = document.createElement('td');
            const amount = Math.floor(line.buxAmount);
            buxTd.innerHTML = `<span class="bux-pill">${amount.toLocaleString()} R$</span>`;
            tr.appendChild(buxTd)

            tbody.appendChild(tr);
        });

        if (result.response.myPlace > 0) {
            myPlaceElem.textContent = `Your place: ${result.response.myPlace}`;
        }
    } else {
        const tr = document.createElement('tr');
        const td = document.createElement('td');
        td.colSpan = 3;
        td.textContent = 'Unable to load leaderboard';
        tr.appendChild(td);
        tbody.appendChild(tr);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    loadLeaderboard();
});
