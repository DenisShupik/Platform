import { mount } from 'svelte'
import App from './App.svelte'
import { initRouter } from '$lib/stores/routeStore'
import keycloak from '$lib/keycloak'
import { updateUser } from '$lib/stores/userStore'
import type { KeycloakTokenParsed } from 'keycloak-js'
import { categoryStore } from '$lib/stores/categoryStore'

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
window.__categoryStore = categoryStore

const app = mount(App, {
  // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
  target: document.getElementById('app')!
})

export default app
