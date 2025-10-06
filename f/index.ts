import { callGetOfferWallAddSlotLink } from './src/api.js';

const FILE = 'index.ts';

document.addEventListener('DOMContentLoaded', () => {
    const ayetLink = document.getElementById('offer-wall-link-ayet') as HTMLAnchorElement | null;
    const surveyFrame = document.getElementById('survey-frame') as HTMLIFrameElement | null;

    if (!ayetLink) {
        console.error(`${FILE}: missing element: offer-wall-link-ayet`);
        return;
    }

    if (!surveyFrame) {
        console.error(`${FILE}: missing element: survey-frame`);
        return;
    }

    const ayetAnchor = ayetLink as HTMLAnchorElement;
    const surveyIframe = surveyFrame as HTMLIFrameElement;

    const fallbackHref = ayetAnchor.getAttribute('href') ?? '';
    let cachedLink: string | null = null;
    let loading = false;

    async function loadOfferwallLink(): Promise<string | null> {
        if (cachedLink) {
            return cachedLink;
        }

        if (loading) {
            return null;
        }

        loading = true;
        try {
            const response = await callGetOfferWallAddSlotLink();
            if (response.response && response.response.link) {
                cachedLink = response.response.link;
                ayetAnchor.href = cachedLink;
                return cachedLink;
            }

            console.error(`${FILE}: failed to get offerwall link`, response.error || response.message);
            return null;
        } catch (err) {
            console.error(`${FILE}: exception while getting offerwall link`, err);
            return null;
        } finally {
            loading = false;
        }
    }

    ayetAnchor.addEventListener('click', async (event) => {
        event.preventDefault();

        const link = await loadOfferwallLink();
        if (link) {
            surveyIframe.src = link;
            return;
        }

        if (fallbackHref) {
            window.open(fallbackHref, '_blank', 'noopener,noreferrer');
        }
    });
});
