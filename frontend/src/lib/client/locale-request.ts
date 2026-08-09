import { getLocale, type Locale } from '$lib/paraglide/runtime'

export function applyLocaleRequestHeader(request: Request, locale: Locale = getLocale()): Request {
	request.headers.set('Accept-Language', locale)
	return request
}
