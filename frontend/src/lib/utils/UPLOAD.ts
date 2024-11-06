import { setAuthHeader } from '$lib/stores/authStore'

export const UPLOAD = async (
  url: string,
  body: FormData,
  options: RequestInit = {}
) => {
  const fullUrl = `https://localhost:8000/api${url}`
  options.method = 'POST'
  options.body = body
  await setAuthHeader(options)
  return fetch(fullUrl, options)
}
