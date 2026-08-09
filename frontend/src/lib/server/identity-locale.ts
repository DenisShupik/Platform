import { decodeJwt } from 'jose'
import { cookieName } from '$lib/paraglide/runtime'
import { isSupportedLocale, type SupportedLocale } from '$lib/i18n'

export function getAccessTokenLocale(accessToken: string): SupportedLocale | undefined {
	try {
		const locale = decodeJwt(accessToken).locale
		return isSupportedLocale(locale) ? locale : undefined
	} catch {
		return undefined
	}
}

export function addLocaleCookieToRequest(request: Request, locale: SupportedLocale): Request {
	const headers = new Headers(request.headers)
	const cookies =
		headers
			.get('cookie')
			?.split(';')
			.map((cookie) => cookie.trim())
			.filter((cookie) => cookie && !cookie.startsWith(`${cookieName}=`)) ?? []
	cookies.push(`${cookieName}=${locale}`)
	headers.set('cookie', cookies.join('; '))
	return new Request(request, { headers })
}
