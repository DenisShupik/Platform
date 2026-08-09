import { getLocale, type Locale } from '$lib/paraglide/runtime'
import * as m from '$lib/paraglide/messages'

export const DISPLAY_TIME_ZONE = 'UTC'

export function formatTimestamp(date: Date, locale: Locale = getLocale()): string {
	return new Intl.DateTimeFormat(locale, {
		year: 'numeric',
		month: '2-digit',
		day: '2-digit',
		hour: '2-digit',
		minute: '2-digit',
		second: '2-digit',
		timeZone: DISPLAY_TIME_ZONE,
		timeZoneName: 'short'
	}).format(date)
}

export function formatDate(date: Date, locale: Locale = getLocale()): string {
	return new Intl.DateTimeFormat(locale, {
		dateStyle: 'short',
		timeZone: DISPLAY_TIME_ZONE
	}).format(date)
}

export function formatNumber(value: number, locale: Locale = getLocale()): string {
	return new Intl.NumberFormat(locale).format(value)
}

export function formatRelativeTimestamp(
	date: Date,
	now = new Date(),
	locale: Locale = getLocale()
): string {
	const seconds = Math.round((date.getTime() - now.getTime()) / 1000)
	const units: ReadonlyArray<[Intl.RelativeTimeFormatUnit, number]> = [
		['year', 31_536_000],
		['month', 2_592_000],
		['week', 604_800],
		['day', 86_400],
		['hour', 3_600],
		['minute', 60]
	]
	const [unit, divisor] = units.find(([, size]) => Math.abs(seconds) >= size) ?? ['second', 1]
	return new Intl.RelativeTimeFormat(locale, { numeric: 'auto' }).format(
		Math.round(seconds / divisor),
		unit
	)
}

export type CountUnit = 'post' | 'thread' | 'category'

export function formatCountUnit(
	count: number,
	unit: CountUnit,
	locale: Locale = getLocale()
): string {
	const category = new Intl.PluralRules(locale).select(count)
	const messages = {
		post: {
			zero: m.stats_post_zero,
			one: m.stats_post_one,
			two: m.stats_post_two,
			few: m.stats_post_few,
			many: m.stats_post_many,
			other: m.stats_post_other
		},
		thread: {
			zero: m.stats_thread_zero,
			one: m.stats_thread_one,
			two: m.stats_thread_two,
			few: m.stats_thread_few,
			many: m.stats_thread_many,
			other: m.stats_thread_other
		},
		category: {
			zero: m.stats_category_zero,
			one: m.stats_category_one,
			two: m.stats_category_two,
			few: m.stats_category_few,
			many: m.stats_category_many,
			other: m.stats_category_other
		}
	} as const
	return messages[unit][category]({}, { locale })
}
