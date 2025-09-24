// last_redeemrs.ts
import {callGetData, Response} from '../../src/api.js';

// ----- API types (camelCase matches ASP.NET default JSON) -----
export interface LastRedeemerDto {
	userId: number;
	userName: string;
	amountRedeemed: number;
	redeemedAt: string; // ISO string
}

export interface LastRedeemersResponse {
	items: LastRedeemerDto[];
}

// ----- API call -----
async function callGetLastRedeemers(count = 10): Promise<Response<LastRedeemersResponse>> {
	return await callGetData<LastRedeemersResponse>(`b/api1/earners/last-redeemers?count=${count}`);
}

// ----- Rendering -----
function renderLoading(container: HTMLElement) {
	container.innerHTML = `<div class="last-redeemers-loading">Loading last winners ...</div>`;
}

function renderError(container: HTMLElement, message: string) {
	container.innerHTML = `<div class="last-redeemers-error">${escapeHtml(message)}</div>`;
}

function renderEmpty(container: HTMLElement) {
	container.innerHTML = `<div class="last-redeemers-empty">No redemptions yet. Be the first!</div>`;
}

function renderList(container: HTMLElement, redeemers: LastRedeemerDto[]) {
	const items = redeemers.map((e, idx) => `
    <div class="last-redeemer-row">
      <div class="rank">${idx + 1}</div>
      <div class="username">${escapeHtml(e.userName ?? '??')}</div>
      <div class="amount">${Math.floor(e.amountRedeemed)} R$</div>
    </div>
  `).join('');

	container.innerHTML = `
    <div id="last-redeemers-widget" class="last-redeemers">
        <h3 class="last-redeemers-title">Last Winners</h3>
        <div class="last-redeemers-container">
            <div class="last-redeemers-list">
              ${items}
            </div>
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
export async function mountLastRedeemers(containerId = 'last-redeemers', count = 10): Promise<void> {
	const container = document.getElementById(containerId);
	if (!container) {
		console.error(`[LastRedeemers] Missing container #${containerId}`);
		return;
	}

	renderLoading(container);

	const res = await callGetLastRedeemers(count);

	if (!res || res.response === null) {
		renderError(container, res?.message || 'Unknown error');
		return;
	}

	const items = res.response.items ?? [];
	if (items.length === 0) {
		renderEmpty(container);
		return;
	}

	renderList(container, items);
}

// Auto-mount if the container exists
document.addEventListener('DOMContentLoaded', () => {
	const el = document.getElementById('last-redeemers');
	if (el) {
		void mountLastRedeemers('last-redeemers');
	}
});

