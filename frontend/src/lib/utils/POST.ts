import { setAuthHeader } from "$lib/stores/authStore"

export const POST = async (
  url: string,
  body: any = {},
  options: RequestInit = {}
) => {
  const fullUrl = `https://localhost:8000/api${url}`
  options.method = 'POST'
  options.headers = {
    'Content-Type': 'application/json',
    ...(options.headers ?? {})
  }
  options.body = JSON.stringify(body)
  await setAuthHeader(options)
  return fetch(fullUrl, options)
}
