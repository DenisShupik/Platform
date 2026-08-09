import { describe, expect, it } from 'vitest'
import { buildAuthErrorUrl } from './auth-error'

describe('auth error bridge', () => {
	it('keeps only the safe error code on the neutral error route', () => {
		const target = buildAuthErrorUrl(
			new URL('https://app.test/api/auth/localized-error?error=access_denied&error_description=raw')
		)

		expect(target.toString()).toBe('https://app.test/auth/error?error=access_denied')
	})

	it('works without locale state', () => {
		expect(buildAuthErrorUrl(new URL('https://app.test/api/auth/localized-error')).toString()).toBe(
			'https://app.test/auth/error'
		)
	})
})
