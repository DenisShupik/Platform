import type { Locale } from '$lib/paraglide/runtime'

export const supportedLocales = ['en', 'ru'] as const satisfies readonly Locale[]
export type SupportedLocale = (typeof supportedLocales)[number]

export function isSupportedLocale(value: unknown): value is SupportedLocale {
	return typeof value === 'string' && supportedLocales.includes(value as SupportedLocale)
}
