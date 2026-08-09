import {
	DISPLAY_TIME_ZONE,
	formatCountUnit,
	formatDate,
	formatNumber,
	formatRelativeTimestamp,
	formatTimestamp
} from '$lib/utils/format'
import { describe, expect, it } from 'vitest'

describe('localized formatting', () => {
	it('formats numbers using the requested locale', () => {
		expect(formatNumber(12_345.67, 'en')).toBe(new Intl.NumberFormat('en').format(12_345.67))
		expect(formatNumber(12_345.67, 'ru')).toBe(new Intl.NumberFormat('ru').format(12_345.67))
	})

	it('uses every applicable Russian cardinal category', () => {
		expect([0, 1, 2, 5, 21].map((count) => formatCountUnit(count, 'thread', 'ru'))).toEqual([
			'тем',
			'тема',
			'темы',
			'тем',
			'тема'
		])
	})

	it('formats relative time from an explicit reference instant', () => {
		const now = new Date('2026-08-09T12:00:00.000Z')
		expect(formatRelativeTimestamp(new Date('2026-08-09T11:58:00.000Z'), now, 'en')).toBe(
			'2 minutes ago'
		)
		expect(formatRelativeTimestamp(new Date('2026-08-09T11:58:00.000Z'), now, 'ru')).toBe(
			'2 минуты назад'
		)
	})

	it('uses one explicit time zone for SSR and browser output', () => {
		const instant = new Date('2026-08-09T23:30:45.000-07:00')
		const expectedTimestamp = new Intl.DateTimeFormat('en', {
			year: 'numeric',
			month: '2-digit',
			day: '2-digit',
			hour: '2-digit',
			minute: '2-digit',
			second: '2-digit',
			timeZone: 'UTC',
			timeZoneName: 'short'
		}).format(instant)
		const expectedDate = new Intl.DateTimeFormat('en', {
			dateStyle: 'short',
			timeZone: 'UTC'
		}).format(instant)

		expect(DISPLAY_TIME_ZONE).toBe('UTC')
		expect(formatTimestamp(instant, 'en')).toBe(expectedTimestamp)
		expect(formatDate(instant, 'en')).toBe(expectedDate)
	})
})
