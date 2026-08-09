import { getRequestEvent } from '$app/server'
import { AUTH_KEYCLOAK_ISSUER, BETTER_AUTH_SECRET, BETTER_AUTH_URL } from '$env/static/private'
import { PUBLIC_AVATAR_URL, PUBLIC_KEYCLOAK_CLIENT_ID } from '$env/static/public'
import { betterAuth } from 'better-auth'
import { genericOAuth, keycloak } from 'better-auth/plugins'
import { sveltekitCookies } from 'better-auth/svelte-kit'
import { getEffectiveRole, Role } from '$lib/roles'
import { parseUserId } from '$lib/utils/value-object'
import { authErrorBridgePath, getAuthLocaleAuthorizationParameters } from '$lib/auth-locale'

const kc = {
	...keycloak({
		clientId: PUBLIC_KEYCLOAK_CLIENT_ID,
		issuer: AUTH_KEYCLOAK_ISSUER,
		scopes: ['openid', 'profile', 'email'],
		clientSecret: '',
		pkce: true
	}),
	issuer: AUTH_KEYCLOAK_ISSUER,
	requireIssuerValidation: true
}

kc.authorizationUrlParams = (context): Record<string, string> => {
	return getAuthLocaleAuthorizationParameters(context.body?.additionalData?.locale)
}

kc.mapProfileToUser = (profile) => {
	const userId = parseUserId(profile.id)
	if (userId === undefined) throw new Error('Keycloak returned an invalid user ID')

	return {
		...profile,
		name: profile.preferred_username,
		userId,
		role: getEffectiveRole(profile.roles),
		avatarUrl: `${PUBLIC_AVATAR_URL}/${userId}`
	}
}

export const auth = betterAuth({
	secret: BETTER_AUTH_SECRET,
	baseURL: BETTER_AUTH_URL,
	onAPIError: {
		errorURL: new URL(authErrorBridgePath, BETTER_AUTH_URL).toString()
	},
	disabledPaths: ['/update-user'],
	user: {
		additionalFields: {
			userId: {
				type: 'string',
				returned: true,
				required: true
			},
			role: {
				type: Object.values(Role),
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
