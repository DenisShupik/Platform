import { describe, expect, it } from 'vitest'
import { getAuthLocaleAuthorizationParameters } from './auth-locale'

describe('authentication locale', () => {
	it.each(['en', 'ru'] as const)('passes supported locale %s to Keycloak', (locale) => {
		expect(getAuthLocaleAuthorizationParameters(locale)).toEqual({ ui_locales: locale })
	})

	it.each([undefined, '', 'de', 'ru-RU'])('rejects unsupported locale %s', (locale) => {
		expect(() => getAuthLocaleAuthorizationParameters(locale)).toThrow(TypeError)
	})
})
