import { setAuthHeader } from "$lib/stores/authStore"

export const GET = async <T>(
  url: string,
  options: RequestInit = {}
): Promise<T> => {
  const fullUrl = `https://localhost:8000/api${url}`
  options.method = 'GET'
  options.headers = {
    'Content-Type': 'application/json',
    ...(options.headers ?? {})
  }
  await setAuthHeader(options)
  const response = await fetch(fullUrl, options)

  if (!response.ok) {
    throw new Error(`Error: ${response.status} ${response.statusText}`)
  }

  return response.json() as Promise<T>
}
