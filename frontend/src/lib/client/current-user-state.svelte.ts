import Keycloak from 'keycloak-js'
import {
	PUBLIC_KEYCLOAK_URL,
	PUBLIC_KEYCLOAK_REALM,
	PUBLIC_KEYCLOAK_CLIENT_ID,
	PUBLIC_AVATAR_URL
} from '$env/static/public'

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

const keycloakState = $state(keycloak)
let avatarUrl = $state<string | undefined>(undefined)
let isRefreshed = $state(false)

export const currentUser = $state<{
	user?: {
		id: string | undefined
		username: string | undefined
		email: string | undefined
		avatarUrl: string | undefined
		token: string | undefined
	}
}>({})

export const setCurrentUserAvatarUrl = (
	id: string | undefined,
	skipCache: boolean | undefined = undefined
) => {
	if (id) {
		avatarUrl = `${PUBLIC_AVATAR_URL}/${id}${skipCache ? `?${Date.now()}` : ''}`
	} else {
		avatarUrl = undefined
	}
}

$effect.root(() => {
	let intervalId: number | undefined = undefined

	$effect(() => {
		if (keycloakState.authenticated) {
			intervalId = setInterval(() => {
				keycloakState
					.updateToken(30)
					.then((refreshed) => {
						if (refreshed) isRefreshed = true
					})
					.catch((e) => {
						console.error('Не удалось обновить токен', e)
					})
			}, 15000)
		} else {
			clearInterval(intervalId)
			intervalId = undefined
		}

		return () => {
			if (intervalId) {
				clearInterval(intervalId)
			}
		}
	})

	$effect(() => {
		setCurrentUserAvatarUrl(keycloakState.authenticated ? keycloakState.subject : undefined)
	})

	$effect(() => {
		if (isRefreshed) isRefreshed = false
		currentUser.user = keycloakState.authenticated
			? {
					id: keycloakState.subject,
					username: keycloakState.tokenParsed?.preferred_username,
					email: keycloakState.tokenParsed?.email,
					avatarUrl,
					token: keycloakState.token
				}
			: undefined
	})
})

export const { login, logout } = keycloak
