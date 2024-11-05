import keycloak from './keycloak'

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
  if (keycloak.authenticated) {
    await keycloak.updateToken(30)
    options.headers = {
      ...options.headers,
      Authorization: `Bearer ${keycloak.token}`
    }
  }
  return fetch(fullUrl, options)
}
