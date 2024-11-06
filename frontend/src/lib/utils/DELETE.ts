import { setAuthHeader } from "$lib/stores/authStore"

export const DELETE = async (
  url: string,
  body: any | undefined = undefined,
  options: RequestInit = {}
) => {
  const fullUrl = `https://localhost:8000/api${url}`
  options.method = 'DELETE'
  if (body != null) {
    options.body = JSON.stringify(body)
  }
  await setAuthHeader(options)
  return fetch(fullUrl, options)
}
