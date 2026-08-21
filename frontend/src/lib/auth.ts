import { getRequestEvent } from '$app/server'
import { AUTH_KEYCLOAK_ISSUER, BETTER_AUTH_SECRET, BETTER_AUTH_URL } from '$env/static/private'
import { PUBLIC_AVATAR_URL, PUBLIC_KEYCLOAK_CLIENT_ID } from '$env/static/public'
import { betterAuth } from 'better-auth'
import { genericOAuth, keycloak, type GenericOAuthUserInfo } from 'better-auth/plugins'
import { sveltekitCookies } from 'better-auth/svelte-kit'
import { parseUserId } from '$lib/utils/value-object'
import { authErrorBridgePath } from '$lib/auth-locale'

const keycloakIssuer = AUTH_KEYCLOAK_ISSUER.replace(/\/$/, '')

const kc = {
	...keycloak({
		clientId: PUBLIC_KEYCLOAK_CLIENT_ID,
		issuer: keycloakIssuer,
		scopes: ['openid', 'profile', 'email'],
		clientSecret: '',
		pkce: true
	}),
	requireIdTokenVerification: true,
	mapProfileToUser(profile: GenericOAuthUserInfo) {
		const userId = parseUserId(profile.id)
		if (userId === undefined) throw new Error('Keycloak returned an invalid user ID')

		const name =
			typeof profile.preferred_username === 'string'
				? profile.preferred_username
				: typeof profile.name === 'string'
					? profile.name
					: userId

		return {
			name,
			email: profile.email,
			emailVerified: profile.emailVerified,
			image: profile.image,
			userId,
			avatarUrl: `${PUBLIC_AVATAR_URL}/${userId}`
		}
	}
}

export function createAuth() {
	return betterAuth({
		secret: BETTER_AUTH_SECRET,
		baseURL: BETTER_AUTH_URL,
		onAPIError: {
			errorURL: new URL(authErrorBridgePath, BETTER_AUTH_URL).toString()
		},
		disabledPaths: ['/update-user'],
		account: {
			storeAccountCookie: true
		},
		user: {
			additionalFields: {
				userId: {
					type: 'string',
					returned: true,
					required: true
				},
				avatarUrl: {
					type: 'string',
					returned: true,
					required: false
				}
			}
		},
		plugins: [
			genericOAuth({
				config: [kc]
			}),
			sveltekitCookies(getRequestEvent)
		]
	})
}

export type Auth = ReturnType<typeof createAuth>
