export const idpConfig = {
  authorizationEndpoint:
    'https://localhost:8443/realms/app/protocol/openid-connect/auth',
  tokenEndpoint:
    'https://localhost:8443/realms/app/protocol/openid-connect/token',
  clientId: 'app-user',
  redirectUri: 'https://localhost:5173'
}

export type IdpConfig = typeof idpConfig
