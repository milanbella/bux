// top_earners.ts
import {callGetData, Response} from '../../src/api.js';

// ----- API types -----
export interface Earner {
	userId: number;
	username: string;
	amount: number;
}

export interface TopEarnersResponse {
	earners: Earner[];
}

// ----- API call -----
async function callGetTopEarners(): Promise<Response<TopEarnersResponse>> {
	return await callGetData<TopEarnersResponse>('b/api1/earners/top');
}

// ----- Rendering -----

function renderLoading(container: HTMLElement) {
	container.innerHTML = `<div class="top-earners-loading">Loading top earners ...</div>`;
}

function renderError(container: HTMLElement, message: string) {
	container.innerHTML = `<div class="top-earners-error"> ${escapeHtml(message)}</div>`;
}

function renderEmpty(container: HTMLElement) {
	container.innerHTML = `<div class="top-earners-empty">No earners yet. Be the first to earn R$!</div>`;
}

function renderList(container: HTMLElement, earners: Earner[]) {
	const items = earners.map((e, idx) => `
    <div class="top-earner-row">
      <div class="rank">${idx + 1}</div>
      <div class="username"> ${escapeHtml(e.username ?? '??')}</div>
      <div class="amount">${Math.floor(e.amount)} R$</div>
    </div>
  `).join('');

	container.innerHTML = `
    <div id="top-earners-widget" class="top-earners">
      <h3 class="top-earners-title">Top Earners</h3>
      <div class="top-earners-list">
        ${items}
      </div>
    </div>
  `;
}

// Minimal HTML escaper
function escapeHtml(s: string): string {
	return s
		.replaceAll('&', '&amp;')
		.replaceAll('<', '&lt;')
		.replaceAll('>', '&gt;')
		.replaceAll('"', '&quot;')
		.replaceAll("'", '&#039;');
}

// ----- Public mount -----
export async function mountTopEarners(containerId = 'top-earners'): Promise<void> {
	const container = document.getElementById(containerId);
	if (!container) {
		console.error(`[TopEarners] Missing container #${containerId}`);
		return;
	}

	renderLoading(container);

	const res = await callGetTopEarners();

	if (!res || res.response === null) {
		renderError(container, res?.message || 'Unknown error');
		return;
	}

	const earners = res.response.earners ?? [];
	if (earners.length === 0) {
		renderEmpty(container);
		return;
	}

	renderList(container, earners);
}

document.addEventListener('DOMContentLoaded', () => {
	const el = document.getElementById('top-earners');
	if (el) {
		void mountTopEarners('top-earners');
	}
});

