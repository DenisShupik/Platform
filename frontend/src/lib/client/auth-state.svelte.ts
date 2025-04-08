import Keycloak from 'keycloak-js'
import {
	PUBLIC_KEYCLOAK_URL,
	PUBLIC_KEYCLOAK_REALM,
	PUBLIC_KEYCLOAK_CLIENT_ID,
	PUBLIC_AVATAR_URL
} from '$env/static/public'
import { writable, readonly, derived } from 'svelte/store'

const keycloak = new Keycloak({
	url: PUBLIC_KEYCLOAK_URL,
	realm: PUBLIC_KEYCLOAK_REALM,
	clientId: PUBLIC_KEYCLOAK_CLIENT_ID
})

try {
	await keycloak.init({
		onLoad: 'check-sso',
		silentCheckSsoRedirectUri: `${location.origin}/silent-check-sso.html`
	})
} catch (error) {
	console.error('Failed to initialize adapter:', error)
}

export const authStore = writable(keycloak)
export const avatarStore = writable<string | undefined>(undefined)

export const setCurrentUserAvatarUrl = (
	id: string | undefined,
	skipCache: boolean | undefined = undefined
) => {
	if (id) {
		avatarStore.set(`${PUBLIC_AVATAR_URL}/${id}${skipCache ? `?${Date.now()}` : ''}`)
	} else {
		avatarStore.set(undefined)
	}
}

let intervalId: number | undefined = undefined

authStore.subscribe((keycloak) => {
	if (keycloak.authenticated) {
		intervalId = setInterval(() => {
			keycloak.updateToken(30).catch(() => {
				//console.error('Не удалось обновить токен', e)
			})
		}, 3000)
	} else {
		clearInterval(intervalId)
		intervalId = undefined
	}
})

authStore.subscribe((keycloak) => {
	setCurrentUserAvatarUrl(keycloak.authenticated ? keycloak.subject : undefined)
})

const currentUserStore = derived([authStore, avatarStore], ([$authStore, $avatarStore]) =>
	$authStore.authenticated
		? {
				id: $authStore.subject,
				username: $authStore.tokenParsed?.preferred_username,
				email: $authStore.tokenParsed?.email,
				avatarUrl: $avatarStore
			}
		: undefined
)

export const currentUser = readonly(currentUserStore)
