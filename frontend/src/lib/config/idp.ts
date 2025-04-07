export const idpConfig = {
  authorizationEndpoint:
    'http://localhost:8080/realms/app-dev/protocol/openid-connect/auth',
  tokenEndpoint:
    'http://localhost:8080/realms/app-dev/protocol/openid-connect/token',
  clientId: 'app-user',
  redirectUri: 'http://localhost:5173'
}

export type IdpConfig = typeof idpConfig
