import { writable } from 'svelte/store'
import { jwtDecode, type JwtPayload } from 'jwt-decode'

export interface CurrentUser {
  userId: string
  username: string
}

export const authStore = writable<CurrentUser | undefined>()

const charset =
  'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~'
const charsetLength = charset.length
const redirectUri = 'https://localhost:5173'
const clientId = 'app-user'
const tokenEndpoint =
  'https://localhost:8443/realms/app/protocol/openid-connect/token'

function base64UrlEncode(array: ArrayBuffer): string {
  return window
    .btoa(
      String.fromCharCode.apply(
        null,
        new Uint8Array(array) as unknown as number[]
      )
    )
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '')
}

function randomString(length: number): string {
  const bytes = new Uint8Array(length)
  window.crypto.getRandomValues(bytes)
  let result = ''
  for (let i = 0; i < length; ++i) {
    result += charset[bytes[i] % charsetLength]
  }
  return result
}

export async function initAuthCodeFlow() {
  const authorizationEndpoint =
    'https://localhost:8443/realms/app/protocol/openid-connect/auth'
  const scope = 'openid profile'
  const state = randomString(16)
  sessionStorage.setItem('state', state)
  const codeVerifier = randomString(43)
  sessionStorage.setItem('codeVerifier', codeVerifier)
  const codeChallenge = base64UrlEncode(
    await window.crypto.subtle.digest(
      'SHA-256',
      new TextEncoder().encode(codeVerifier)
    )
  )
  const authUrl = `${authorizationEndpoint}?response_type=code&client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&scope=${scope}&state=${state}&code_challenge=${codeChallenge}&code_challenge_method=S256`
  window.location.href = authUrl
}

let accessToken: string
let refreshToken: string
let decoded: JwtPayload

export async function exchange() {
  const urlParams = new URLSearchParams(window.location.search)
  const authCode = urlParams.get('code')
  const state = urlParams.get('state')
  const expectedState = sessionStorage.getItem('state')
  const codeVerifier = sessionStorage.getItem('codeVerifier')
  if (state !== expectedState) {
    console.error('State mismatch')
    return
  }

  const response = await fetch(tokenEndpoint, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    body: new URLSearchParams({
      grant_type: 'authorization_code',
      code: authCode,
      redirect_uri: redirectUri,
      client_id: clientId,
      code_verifier: codeVerifier
    })
  })
  const data: { access_token: string; refresh_token: string } =
    await response.json()
  accessToken = data.access_token
  refreshToken = data.refresh_token
  decoded = jwtDecode(data.access_token)
  authStore.set({
    userId: decoded.sub,
    username: decoded.preferred_username
  })
}

let refreshPromise: Promise<void> | undefined = undefined

function refresh() {
  if (refreshPromise) {
    return refreshPromise
  }

  refreshPromise = fetch(tokenEndpoint, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    body: new URLSearchParams({
      client_id: clientId,
      grant_type: 'refresh_token',
      refresh_token: refreshToken
    })
  })
    .then((response) => response.json())
    .then((data) => {
      accessToken = data.access_token
      refreshToken = data.refresh_token
      decoded = jwtDecode(data.access_token)
      authStore.set({
        userId: decoded.sub,
        username: decoded.preferred_username
      })
    })
    .catch((error) => {
      console.error('Ошибка при запросе:', error)
      throw error // Пробрасываем ошибку для обработки
    })
    .finally(() => {
      refreshPromise = undefined
    })
}

export async function setAuthHeader(options: RequestInit) {
  if (Math.floor(Date.now() / 1000) > decoded.exp) {
    await refresh()
  }
  options.headers = {
    ...options.headers,
    Authorization: `Bearer ${accessToken}`
  }
}
