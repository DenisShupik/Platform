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

export const authState = $state({ keycloak })
const currentUser = $derived(
	authState.keycloak.authenticated
		? {
				avatarUrl: authState.keycloak.subject
					? `${PUBLIC_AVATAR_URL}/${authState.keycloak.subject}`
					: undefined
			}
		: undefined
)

export function getCurrentUser() {
	return currentUser
}
