import { AUTH_KEYCLOAK_ISSUER } from '$env/static/private'
import { PUBLIC_AVATAR_URL, PUBLIC_KEYCLOAK_CLIENT_ID } from '$env/static/public'
import { SvelteKitAuth } from '@auth/sveltekit'
import type { AdapterUser } from '@auth/sveltekit/adapters'
import Keycloak from '@auth/sveltekit/providers/keycloak'

declare module '@auth/sveltekit' {
	interface Session {
		error?: 'RefreshTokenError'
		user: AdapterUser & User
	}
}

declare module '@auth/sveltekit' {
	interface User {
		userId?: string
		avatarUrl?: string
		access_token?: string
	}
}

declare module '@auth/sveltekit' {
	interface JWT {
		access_token: string
		expires_at: number
		refresh_token?: string
		error?: 'RefreshTokenError'
	}
}

export const { handle, signIn, signOut } = SvelteKitAuth({
	providers: [Keycloak],
	callbacks: {
		async jwt({ token, account, profile }) {
			if (account && profile) {
				token.userId = profile.sub
				return {
					...token,
					access_token: account.access_token,
					expires_at: account.expires_at,
					refresh_token: account.refresh_token
				}
			} else if (Date.now() < token.expires_at * 1000) {
				return token
			} else {
				if (!token.refresh_token) throw new TypeError('Missing refresh_token')
				try {
					const tokenEndpoint = `${AUTH_KEYCLOAK_ISSUER}/protocol/openid-connect/token`
					const response = await fetch(tokenEndpoint, {
						method: 'POST',
						body: new URLSearchParams({
							client_id: PUBLIC_KEYCLOAK_CLIENT_ID,
							grant_type: 'refresh_token',
							refresh_token: token.refresh_token!
						})
					})

					const tokensOrError = await response.json()

					if (!response.ok) throw tokensOrError

					const newTokens = tokensOrError as {
						access_token: string
						expires_in: number
						refresh_token?: string
					}

					return {
						...token,
						access_token: newTokens.access_token,
						expires_at: Math.floor(Date.now() / 1000 + newTokens.expires_in),
						// Some providers only issue refresh tokens once, so preserve if we did not get a new one
						refresh_token: newTokens.refresh_token ? newTokens.refresh_token : token.refresh_token
					}
				} catch (error) {
					console.error('Error refreshing access_token', error)
					// If we fail to refresh the token, return an error so we can handle it on the page
					token.error = 'RefreshTokenError'
					return token
				}
			}
		},
		async session({ session, token }) {
			session.user.userId = token.userId
			session.user.avatarUrl = `${PUBLIC_AVATAR_URL}/${token.userId}`
			session.access_token = token.access_token
			session.error = token.error
			return session
		}
	}
})
