import { vCreateForumRequestBody } from '$lib/utils/client/valibot.gen'
import { safeParse } from 'valibot'
import { describe, expect, it } from 'vitest'
import '@valibot/i18n/ru'

describe('localized validation', () => {
	it.each([
		['en', 'Invalid length: Expected >=3 but received 0'],
		['ru', 'Неправильная длина: ожидалось >=3, получено 0']
	] as const)('uses the %s catalog for generated schemas', (locale, expectedMessage) => {
		const result = safeParse(vCreateForumRequestBody, { title: '' }, { lang: locale })

		expect(result.success).toBe(false)
		expect(result.issues?.[0]?.message).toBe(expectedMessage)
	})
})
