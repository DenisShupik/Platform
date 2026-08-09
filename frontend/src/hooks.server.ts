import { sequence } from '@sveltejs/kit/hooks'
import { cookieMaxAge, cookieName, getTextDirection } from '$lib/paraglide/runtime'
import { paraglideMiddleware } from '$lib/paraglide/server'
import { auth } from '$lib/auth'
import { parseUserId } from '$lib/utils/value-object'
import { svelteKitHandler } from 'better-auth/svelte-kit'
import { building } from '$app/environment'
import type { Handle } from '@sveltejs/kit'
import '@valibot/i18n/ru'
import { authErrorBridgePath, buildAuthErrorUrl } from '$lib/server/auth-error'
import { isSupportedLocale } from '$lib/i18n'
import { addLocaleCookieToRequest, getAccessTokenLocale } from '$lib/server/identity-locale'

function isBetterAuthPath(pathname: string): boolean {
	return pathname === '/api/auth' || pathname.startsWith('/api/auth/')
}

const originalHandle: Handle = async ({ event, resolve }) => {
	if (event.url.pathname === authErrorBridgePath) {
		const target = buildAuthErrorUrl(event.url)
		return new Response(null, { status: 303, headers: { location: target.toString() } })
	}

	const session = await auth.api.getSession({ headers: event.request.headers })

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

			const explicitLocale = event.cookies.get(cookieName)
			const tokenLocale = getAccessTokenLocale(accessToken.accessToken)
			if (
				event.request.method === 'GET' &&
				!isSupportedLocale(explicitLocale) &&
				tokenLocale !== undefined
			) {
				event.cookies.set(cookieName, tokenLocale, {
					path: '/',
					maxAge: cookieMaxAge,
					httpOnly: false,
					sameSite: 'lax',
					secure: event.url.protocol === 'https:'
				})
				event.request = addLocaleCookieToRequest(event.request, tokenLocale)
			}
		}
	} catch {
		// Ignore errors
	}

	return svelteKitHandler({ event, resolve, auth, building })
}

const handleParaglide: Handle = ({ event, resolve }) => {
	if (isBetterAuthPath(event.url.pathname)) return resolve(event)

	return paraglideMiddleware(event.request, async ({ request, locale }) => {
		event.request = request

		const response = await resolve(event, {
			transformPageChunk: ({ html }) =>
				html
					.replace('%paraglide.lang%', locale)
					.replace('%paraglide.dir%', getTextDirection(locale))
		})
		const headers = new Headers(response.headers)
		headers.set('Content-Language', locale)
		const vary = new Set(
			headers
				.get('Vary')
				?.split(',')
				.map((value) => value.trim())
				.filter(Boolean) ?? []
		)
		vary.add('Cookie')
		vary.add('Accept-Language')
		headers.set('Vary', [...vary].join(', '))
		return new Response(response.body, {
			status: response.status,
			statusText: response.statusText,
			headers
		})
	})
}

export const handle = sequence(originalHandle, handleParaglide)
