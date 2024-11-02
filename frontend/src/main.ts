import { mount } from 'svelte'
import App from './App.svelte'
import { initRouter } from '$lib/stores/routeStore'
import keycloak from '$lib/keycloak'
import { updateUser } from '$lib/stores/currentUserStore'
import type { KeycloakTokenParsed } from 'keycloak-js'

initRouter()

try {
  await keycloak.init({
    onLoad: 'login-required'
  })
  if (keycloak.authenticated != null && keycloak.authenticated)
    updateUser({
      username:
        (
          keycloak.tokenParsed as
            | (KeycloakTokenParsed & { preferred_username?: string })
            | undefined
        )?.preferred_username ?? ''
    })
} catch (error) {
  console.error('Ошибка авторизации:', error)
}

const app = mount(App, {
  // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
  target: document.getElementById('app')!
})

export default app
