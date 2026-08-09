import { describe, expect, it } from 'vitest'
import { cookieName } from '$lib/paraglide/runtime'
import { addLocaleCookieToRequest, getAccessTokenLocale } from '$lib/server/identity-locale'

function unsignedToken(payload: Record<string, unknown>): string {
	const encode = (value: object) => Buffer.from(JSON.stringify(value)).toString('base64url')
	return `${encode({ alg: 'none' })}.${encode(payload)}.`
}

describe('identity locale', () => {
	it.each(['en', 'ru'] as const)('accepts exact supported token locale %s', (locale) => {
		expect(getAccessTokenLocale(unsignedToken({ locale }))).toBe(locale)
	})

	it.each(['ru-RU', 'de', '', 7, undefined])('rejects unsupported token locale %s', (locale) => {
		expect(getAccessTokenLocale(unsignedToken({ locale }))).toBeUndefined()
	})

	it('rejects a malformed access token', () => {
		expect(getAccessTokenLocale('not-a-jwt')).toBeUndefined()
	})

	it('replaces only the locale cookie used for the current SSR request', () => {
		const request = new Request('http://localhost/', {
			headers: { cookie: `session=abc; ${cookieName}=invalid; theme=dark` }
		})

		const localizedRequest = addLocaleCookieToRequest(request, 'ru')

		expect(localizedRequest.headers.get('cookie')).toBe(`session=abc; theme=dark; ${cookieName}=ru`)
	})
})
