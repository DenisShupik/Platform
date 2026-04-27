import { getRequestEvent } from '$app/server'
import { AUTH_KEYCLOAK_ISSUER, BETTER_AUTH_SECRET, BETTER_AUTH_URL } from '$env/static/private'
import { PUBLIC_AVATAR_URL, PUBLIC_KEYCLOAK_CLIENT_ID } from '$env/static/public'
import { betterAuth } from 'better-auth'
import { genericOAuth, keycloak } from 'better-auth/plugins'
import { sveltekitCookies } from 'better-auth/svelte-kit'
import { getEffectiveRole, Role } from '$lib/roles'

const kc = keycloak({
	clientId: PUBLIC_KEYCLOAK_CLIENT_ID,
	issuer: AUTH_KEYCLOAK_ISSUER,
	scopes: ['openid', 'profile', 'email'],
	clientSecret: '',
	pkce: true
})

kc.mapProfileToUser = (profile) => {
	return {
		...profile,
		name: profile.preferred_username,
		userId: profile.id,
		role: getEffectiveRole(profile.roles),
		avatarUrl: `${PUBLIC_AVATAR_URL}/${profile.id}`
	}
}

export const auth = betterAuth({
	secret: BETTER_AUTH_SECRET,
	baseURL: BETTER_AUTH_URL,
	user: {
		additionalFields: {
			userId: {
				type: 'string',
				input: false,
				returned: true,
				required: true
			},
			role: {
				type: Object.values(Role),
				input: false,
				returned: true,
				required: true
			},
			avatarUrl: {
				type: 'string',
				input: false,
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
