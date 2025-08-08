export interface ApiResponse {
  [key: string]: unknown;
}

export async function postData(url: string, data: Record<string, unknown>): Promise<ApiResponse> {
  const response = await fetch(url, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(data)
  });

  return response.json();
}
