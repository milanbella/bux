export interface BrowserRegisterRequest {
    username: string;
    captcha: string;
    referralCode: string;
}

export interface BrowserRegisterResponse {
    username: string;
    error: string;
    message: string;
}

export interface GetUserResponse {
    isLoggedIn: boolean;
    username: string;
    userEmail: string;
    firstName: string;
    lastName: string;
}

export interface GetTotalBuxEarnedResponse {
    totalBux: number;
}

export interface GetAvatar64Response {
    url64: string;
}

export interface GetReferralsCountResponse {
    referralsCount: number;
}

export interface Response<T> {
    response: T | null;
    status: number | null;
    error: string;
    message: string;
}

function set401() {
	if (window.location.pathname === "/account.html") {
		return;
	}
    window.location.href = "/account.html";
    /*
	const main = document.querySelector("main");

	if (!main) {
		console.error('missing main element');
		return;
	}

	// remove all children
	main.innerHTML = "";

	// create anchor element
	const link = document.createElement('a');
	link.id = 'link-to-account-page'
	link.href = 'account.html';
	link.textContent = 'Please set first your roblox name by clicking to this link.';

	const container = document.createElement('div');
	container.id = 'link-to-account-page-container';
    container.appendChild(link);

	// append the anchor to <main>
	main.appendChild(container);
    */
}

export async function callGetData<T>(url: string): Promise<Response<T>> {
    try {
        const response = await fetch(url, {
            method: 'GET'
        });
        if (!response.ok) {
            console.error(`http get failed: ${url}, status ${response.status}`);
            if (response.status ===  401) {
				set401();
            }
            try {
                let responseAny = await response.json();
                return {
                    response: null,
                    status: response.status,
                    error: responseAny.error || '',
                    message: responseAny.message || '',
                }
            } catch {
                let text = await response.text();
                return {
                    response: null,
                    status: response.status,
                    error: text || '',
                    message: text || '',
                }
            }
        }
        let responseT: T = await response.json() as T;
        return {
            response: responseT,
            status: response.status,
            error: '',
            message: '',
        }
    } catch (err) {
        console.error(`http get failed: ${url}`, err);
        return {
            response: null,
            status: null,
            error: 'error',
            message: `http get failed: ${url}`,
        }
    }
}

export async function callPostData<T>(url: string, data: string): Promise<Response<T>> {
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: data
        });
        if (!response.ok) {
            console.error(`http post failed: ${url}, status ${response.status}`);
            if (response.status ===  401) {
				set401();
            }
            try {
                let responseAny = await response.json();
                return {
                    response: null,
                    status: response.status,
                    error: responseAny.error || '',
                    message: responseAny.message || '',
                }
            } catch {
                let text = await response.text();
                return {
                    response: null,
                    status: response.status,
                    error: text || '',
                    message: text || '',
                }
            }
        }
        let responseT: T = await response.json() as T;
        return {
            response: responseT,
            status: response.status,
            error: '',
            message: '',
        }
    } catch (err) {
        console.error(`http post failed: ${url}`, err);
        return {
            response: null,
            status: null,
            error: 'error',
            message: `http post failed: ${url}`,
        }
    }
}

export async function callBrowserRegister(request: BrowserRegisterRequest): Promise<Response<BrowserRegisterResponse>> {
    let data: string = JSON.stringify(request);
    let response = await callPostData<BrowserRegisterResponse>('b/auth/browser-register', data);
    return response;
}

export async function callGetUser(): Promise<Response<GetUserResponse>> {
    let response = await callGetData<GetUserResponse>('b/auth/get-user');
    return response;
}

export async function callGetTotalBuxEarned(): Promise<Response<GetTotalBuxEarnedResponse>> {
    let response = await callGetData<GetTotalBuxEarnedResponse>('b/api/user/get-total-bux-earned');
    return response;
}

export async function callGetAavatar64(): Promise<Response<GetAvatar64Response>> {
    let response = await callGetData<GetAvatar64Response>('b/api/user/get-avatar-64');
    return response;
}

export async function callGetReferralsCount(): Promise<Response<GetReferralsCountResponse>> {
    let response = await callGetData<GetReferralsCountResponse>('b/api/user/get-referrals-count');
    return response;
}
