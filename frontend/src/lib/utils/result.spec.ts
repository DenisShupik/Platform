import { describe, expect, it } from 'vitest'
import { getResultValue, getSuccessfulResultMap } from './result'

describe('API result helpers', () => {
	it('returns only successful bulk result entries', () => {
		const results = {
			first: { value: 1 },
			second: { error: { $type: 'NotFoundError' } },
			third: undefined
		}

		expect(getSuccessfulResultMap(results)).toEqual(new Map([['first', 1]]))
	})

	it('gets a single successful value without hiding it behind an unsafe property access', () => {
		expect(getResultValue({ value: 1 })).toBe(1)
		expect(getResultValue({ error: { $type: 'NotFoundError' } })).toBeUndefined()
		expect(getResultValue(undefined)).toBeUndefined()
	})
})
