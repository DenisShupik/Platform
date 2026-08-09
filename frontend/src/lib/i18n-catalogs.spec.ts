import en from '../../messages/en.json'
import ru from '../../messages/ru.json'
import { describe, expect, it } from 'vitest'

function messageKeys(catalog: Record<string, string>): string[] {
	return Object.keys(catalog)
		.filter((key) => key !== '$schema')
		.sort()
}

function placeholders(message: string): string[] {
	return [...message.matchAll(/\{([A-Za-z_]\w*)\}/g)].map((match) => match[1]).sort()
}

describe('message catalogs', () => {
	it('contain the same complete set of non-empty messages', () => {
		expect(messageKeys(ru)).toEqual(messageKeys(en))
		expect(Object.values(en).every((message) => message.trim().length > 0)).toBe(true)
		expect(Object.values(ru).every((message) => message.trim().length > 0)).toBe(true)
	})

	it('always presents language names as autonyms', () => {
		expect(en.language_english).toBe('English')
		expect(ru.language_english).toBe('English')
		expect(en.language_russian).toBe('Русский')
		expect(ru.language_russian).toBe('Русский')
	})

	it('keeps placeholders identical across locales', () => {
		for (const key of messageKeys(en)) {
			expect(placeholders(ru[key as keyof typeof ru])).toEqual(
				placeholders(en[key as keyof typeof en])
			)
		}
	})

	it('uses a grammatically neutral remaining-character label for every count', () => {
		expect(en.post_characters_remaining).toBe('Characters remaining: {count}')
		expect(ru.post_characters_remaining).toBe('Осталось символов: {count}')
	})
})
