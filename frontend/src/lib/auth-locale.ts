import { isSupportedLocale, type SupportedLocale } from '$lib/i18n'

export const authErrorBridgePath = '/api/auth/localized-error'

export function getAuthLocaleAuthorizationParameters(locale: unknown): {
	ui_locales: SupportedLocale
} {
	if (!isSupportedLocale(locale)) {
		throw new TypeError('A supported authentication locale is required')
	}

	return { ui_locales: locale }
}
