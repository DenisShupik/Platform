import { typedEntries } from './typed-entries'

type ResultValue<T> = { value: T } | { error: unknown }

type ResultKey<T extends object> = Extract<keyof T, string>

type SuccessfulResultValue<T> =
	Extract<Exclude<T, undefined>, { value: unknown }> extends {
		value: infer V
	}
		? V
		: never

export function getResultValue<T>(result: ResultValue<T> | undefined): T | undefined {
	return result && 'value' in result ? result.value : undefined
}

/** Returns only successful values when a bulk consumer intentionally ignores per-item errors. */
export function getSuccessfulResultMap<T extends object>(
	results: T
): Map<ResultKey<T>, SuccessfulResultValue<T[ResultKey<T>]>> {
	const values = new Map<ResultKey<T>, SuccessfulResultValue<T[ResultKey<T>]>>()

	for (const [key, result] of typedEntries(results)) {
		const value = getResultValue(result as ResultValue<unknown> | undefined)
		if (value === undefined) continue

		values.set(key, value as SuccessfulResultValue<T[ResultKey<T>]>)
	}

	return values
}
