import { jwtDecode, type JwtPayload } from 'jwt-decode'
import { writable } from 'svelte/store'

import { idpConfig } from '$lib/config/idp'
import { avatarUrl } from '$lib/env'
import { base64UrlEncode, randomString } from '$lib/utils/pkce'

export interface CurrentUser {
  userId: string
  username: string
  email: string
  avatarUrl?: string
}

export const authStore = writable<CurrentUser | undefined>()

export async function initAuthCodeFlow() {
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
  const authUrl = `${idpConfig.authorizationEndpoint}?response_type=code&client_id=${idpConfig.clientId}&redirect_uri=${encodeURIComponent(idpConfig.redirectUri)}&scope=${scope}&state=${state}&code_challenge=${codeChallenge}&code_challenge_method=S256`
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

  const response = await fetch(idpConfig.tokenEndpoint, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    body: new URLSearchParams({
      grant_type: 'authorization_code',
      code: authCode,
      redirect_uri: idpConfig.redirectUri,
      client_id: idpConfig.clientId,
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
    username: decoded.preferred_username,
    email: decoded.email,
    avatarUrl: `${avatarUrl}${decoded.sub}`
  })
}

let refreshPromise: Promise<void> | undefined = undefined

function refresh() {
  if (refreshPromise) {
    return refreshPromise
  }

  refreshPromise = fetch(idpConfig.tokenEndpoint, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    body: new URLSearchParams({
      client_id: idpConfig.clientId,
      grant_type: 'refresh_token',
      refresh_token: refreshToken
    })
  })
    .then((response) => response.json())
    .then((data) => {
      accessToken = data.access_token
      refreshToken = data.refresh_token
      decoded = jwtDecode(data.access_token)
      authStore.update((e) => {
        if (e == null)
          return {
            userId: decoded.sub,
            username: decoded.preferred_username,
            email: decoded.email,
            avatarUrl: `${avatarUrl}${decoded.sub}`
          }
        e.userId = decoded.sub
        e.username = decoded.preferred_username
        e.email = decoded.email
        return e
      })
    })
    .catch((error: unknown) => {
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
