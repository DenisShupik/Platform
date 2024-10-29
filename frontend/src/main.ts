import { mount } from 'svelte'
import App from './App.svelte'
import { initRouter } from '$lib/routeStore';
import keycloak from '$lib/keycloak'
import { updateUser } from '$lib/stores/userStore';

initRouter();

try {
  await keycloak.init({
    onLoad: 'login-required'
  })
  if (keycloak.authenticated != null && keycloak.authenticated)
    updateUser({ username: keycloak.tokenParsed?.preferred_username || '' })
} catch (error) {
  console.error('Ошибка авторизации:', error)
}

const app = mount(App, {
  target: document.getElementById('app')!,
})

export default app
