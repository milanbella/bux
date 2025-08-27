import {callGetData, callPostData, Response} from './api.js';

interface GetAvatarsResponse {
    url64: string;
    url128: string;
    url256: string;
}

export async function callGetAvatars(): Promise<Response<GetAvatarsResponse>> {
    let response = await callGetData<GetAvatarsResponse>('b/api/user/get-avatars');
    return response;
}


async function main()
{
    let avatars = await callGetAvatars()
    if (avatars.response == null) {
        console.error("error while getting avatars");
        return;
    }

    if (avatars.response.url64) {
        let div64 = document.getElementById('avatar-64')
        if (div64) {
            let img = document.createElement("img");
            img.id = "img-64"; // set id attribute
            img.src = avatars.response.url64;
            img.alt = "Avatar 64x64";
            div64.appendChild(img);
        }
    }

    if (avatars.response.url128) {
        let div128 = document.getElementById('avatar-128')
        if (div128) {
            let img = document.createElement("img");
            img.id = "img-128"; // set id attribute
            img.src = avatars.response.url128;
            img.alt = "Avatar 128x128";
            div128.appendChild(img);
        }
    }

    if (avatars.response.url256) {
        let div256 = document.getElementById('avatar-256')
        if (div256) {
            let img = document.createElement("img");
            img.id = "img-256"; // set id attribute
            img.src = avatars.response.url256;
            img.alt = "Avatar 256x256";
            div256.appendChild(img);
        }
    }
}

main();
