import { getLocale, type Locale } from '$lib/paraglide/runtime'

type ApiOptions = Record<string, unknown> & {
	headers?: HeadersInit
	throwOnError: boolean
}
type LocalizedHeaders = Record<string, string> & { 'Accept-Language': Locale }
type WithLocalizedHeaders<T extends ApiOptions> = Omit<T, 'headers'> & {
	headers: LocalizedHeaders
}

export function withApiLocale<T extends ApiOptions>(
	options: T & { throwOnError: true }
): WithLocalizedHeaders<T> & { throwOnError: true }
export function withApiLocale<T extends ApiOptions>(
	options: T & { throwOnError: false }
): WithLocalizedHeaders<T> & { throwOnError: false }
export function withApiLocale<T extends ApiOptions>(options: T): WithLocalizedHeaders<T>
export function withApiLocale<T extends ApiOptions>(options: T): WithLocalizedHeaders<T> {
	const headers = Object.fromEntries(new Headers(options.headers))
	return {
		...options,
		headers: { ...headers, 'Accept-Language': getLocale() }
	}
}
