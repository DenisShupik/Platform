import keycloak from './keycloak'

export const UPLOAD = async (
    url: string,
    body: FormData,
    options: RequestInit = {}
  ) => {
    const fullUrl = `https://localhost:8000/api${url}`
    options.method = 'POST'
    options.body = body
    if (keycloak.authenticated) {
      await keycloak.updateToken(30)
      options.headers = {
        ...options.headers,
        Authorization: `Bearer ${keycloak.token}`
      }
    }
    return fetch(fullUrl, options)
  }
  