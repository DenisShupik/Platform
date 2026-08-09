import { auth } from '$lib/auth'
import { parseUserId } from '$lib/utils/value-object'
import { svelteKitHandler } from 'better-auth/svelte-kit'
import { building } from '$app/environment'
import type { Handle } from '@sveltejs/kit'

export const handle: Handle = async ({ event, resolve }) => {
	const session = await auth.api.getSession({
		headers: event.request.headers
	})

	if (session) {
		event.locals.role = session.user.role
		const userId = parseUserId(session.user.userId)
		if (userId !== undefined) event.locals.userId = userId
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
