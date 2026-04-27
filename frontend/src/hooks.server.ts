import { auth } from '$lib/auth'
import { svelteKitHandler } from 'better-auth/svelte-kit'
import { building } from '$app/environment'

export async function handle({ event, resolve }) {
	const session = await auth.api.getSession({
		headers: event.request.headers
	})

	if (session) {
		event.locals.role = session.user.role
		event.locals.userId = session.user.userId
	}

	try {
		const accessToken = await auth.api.getAccessToken({
			body: { providerId: 'keycloak' },
			headers: event.request.headers
		})

		if (accessToken) {
			event.locals.accessToken = accessToken.accessToken
		}
	} catch {
		// Ignore errors
	}

	return svelteKitHandler({ event, resolve, auth, building })
}
