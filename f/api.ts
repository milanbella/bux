export interface BrowserRegisterRequest {
    username: string;
    captcha: string;
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

export interface Response<T> {
    response: T | null;
    status: number | null;
    error: string;
    message: string;
}

export async function callGetData<T>(url: string): Promise<Response<T>> {
    try {
        const response = await fetch(url, {
            method: 'GET'
        });
        if (!response.ok) {
            console.error(`http get failed: ${url}, status ${response.status}`);
            let responseAny = await response.json();
            return {
                response: null,
                status: response.status,
                error: responseAny.error || '',
                message: responseAny.message || '',
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
            let responseAny = await response.json();
            return {
                response: null,
                status: response.status,
                error: responseAny.error || '',
                message: responseAny.message || '',
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
