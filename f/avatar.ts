import { displayAavatar } from './header_footer.js';
import { showMessage, clearMessage } from './ui.js';

type Variants = Record<number, Blob>;

const formElement = document.getElementById('avatar-form') as HTMLFormElement | null; 
const input = document.getElementById('avatar-input') as HTMLInputElement | null;
const uploadBtn = document.getElementById('upload-btn') as HTMLButtonElement | null;
const preview = document.getElementById('preview') as HTMLDivElement | null;

if (!formElement) {
	throw new Error('missin avatar-form DOM element');
}

if (!input) {
	throw new Error('missin avatar-input DOM element');
}

if (!uploadBtn) {
	throw new Error('missin upload-btn DOM element');
}

if (!preview) {
	throw new Error('missin preview DOM element');
}

clearMessage();


const ALLOWED = new Set<string>(['image/jpeg', 'image/png', 'image/webp']);
const MAX_BYTES = 20 * 1024 * 1024; // 20MB
const SIZES = [256, 128, 64] as const;

let blobsBySize: Variants | null = null;

input.addEventListener('change', async () => {
	const file = input.files?.[0] ?? null;
	blobsBySize = null;
	preview.innerHTML = '';
	uploadBtn.disabled = true;

	if (!file) {
		console.warn('no file selected');
		return;
	}

	if (!ALLOWED.has(file.type)) {
		console.warn(`Unsupported file type: ${file.type}`);
		alert(`Unsupported file type: ${file.type}`);
		return;
	}
	if (file.size > MAX_BYTES) {
		console.warn('File too large.');
		alert('File too large.');
		return;
	}

	try {
		const { variants } = await processAvatar(file, SIZES);
		blobsBySize = variants;

		// Preview largest variant
		const largest = Math.max(...SIZES);
		const url = URL.createObjectURL(variants[largest]);
		const img = new Image();
		img.src = url;
		img.width = 128;
		img.height = 128;
		img.style.borderRadius = '50%';
		img.onload = () => URL.revokeObjectURL(url);
		preview.appendChild(img);

		uploadBtn.disabled = false;
	} catch (e) {
		console.error(e);
		alert('Failed to process image.');
	}
});

uploadBtn.addEventListener('click', async () => {
	if (!blobsBySize) return;

	const form = new FormData();
	form.append('avatar256', blobsBySize[256], 'avatar-256.jpg');
	form.append('avatar128', blobsBySize[128], 'avatar-128.jpg');
	form.append('avatar64', blobsBySize[64], 'avatar-64.jpg');

	const res = await fetch('/b/api/user/avatar-upload', {method: 'POST', body: form});

	if (!res.ok) {
        console.error('upload failed');
		return;
	}
	const json = await res.json();
	console.log('Uploaded:', json);
    displayAavatar();
    showMessage('info', 'avatar successfully uploaded');
    formElement.remove();
});

/**
 * Process an image File into square avatars at the requested sizes (JPEG).
 */
async function processAvatar(
	file: File,
	sizes: readonly number[]
): Promise<{variants: Variants; previewUrl: string}> {
	// Decode file
	const bitmap = await createImageBitmap(file);

	// Center square crop region
	const side = Math.min(bitmap.width, bitmap.height);
	const sx = Math.floor((bitmap.width - side) / 2);
	const sy = Math.floor((bitmap.height - side) / 2);

	// Source square canvas
	const sourceCanvas = document.createElement('canvas');
	sourceCanvas.width = side;
	sourceCanvas.height = side;
	const sctx = sourceCanvas.getContext('2d', {alpha: true});
	if (!sctx) {
		console.error('2D context unavailable.');
		throw new Error('2D context unavailable.');
	}
	sctx.drawImage(bitmap, sx, sy, side, side, 0, 0, side, side);

	const variants: Variants = {};

	for (const size of sizes) {
		const canvas = document.createElement('canvas');
		canvas.width = size;
		canvas.height = size;
		const ctx = canvas.getContext('2d', {alpha: true});
		if (!ctx) {
			console.error('2D context unavailable.');
			throw new Error('2D context unavailable.');
		}

		ctx.imageSmoothingEnabled = true;
		ctx.imageSmoothingQuality = 'high';
		ctx.drawImage(sourceCanvas, 0, 0, side, side, 0, 0, size, size);

		// Encode to JPEG; switch to 'image/webp' or 'image/png' if preferred
		const blob = await canvasToBlob(canvas, 'image/jpeg', 0.82);
		variants[size] = blob;
	}

	const largest = Math.max(...sizes);
	const previewUrl = URL.createObjectURL(variants[largest]);
	return {variants, previewUrl};
}

function canvasToBlob(
	canvas: HTMLCanvasElement,
	type: string = 'image/jpeg',
	quality?: number
): Promise<Blob> {
	return new Promise<Blob>((resolve, reject) => {
		canvas.toBlob((b) => (b ? resolve(b) : reject(new Error('toBlob failed'))), type, quality);
	});
}

// Make this a module even if imported via <script type="module">
export {};

