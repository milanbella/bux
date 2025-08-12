export interface BrowserRegisterRequest {
    username: string;
}

export interface BrowserRegisterResponse {
    error: string;
    message: string;
}

export interface Response<T> {
    response: T | null;
    status: number;
    error: string;
    message: string;
}

export async function callPostData<T>(url: string, data: string): Promise<Response<T>> {
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
}

export async function callBrowserRegister(request: BrowserRegisterRequest): Promise<Response<BrowserRegisterResponse>> {
    let data: string = JSON.stringify(request);
    let response = await callPostData<BrowserRegisterResponse>('b/auth/browser-register', data);
    return response;
}
