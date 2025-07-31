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

// 1. Создаем обычное состояние для currentUser, которое можно экспортировать
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
				keycloakState.updateToken(30).catch(() => {
					//console.error('Не удалось обновить токен', e)
				})
			}, 3000)
		} else {
			clearInterval(intervalId)
			intervalId = undefined
		}

		// Cleanup функция
		return () => {
			if (intervalId) {
				clearInterval(intervalId)
			}
		}
	})

	// Эффект для обновления аватара при изменении аутентификации
	$effect(() => {
		setCurrentUserAvatarUrl(keycloakState.authenticated ? keycloakState.subject : undefined)
	})

	// 2. Создаем эффект, который обновляет currentUser при изменении зависимостей
	$effect(() => {
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

// Эффект для управления обновлением токена

export const login = keycloakState.login
export const logout = keycloakState.logout
