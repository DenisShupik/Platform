import { describe, expect, it, vi } from 'vitest'

const authMocks = vi.hoisted(() => ({ createAuth: vi.fn() }))

vi.mock('$lib/auth', () => ({ createAuth: authMocks.createAuth }))

import { getAuth } from './auth'

describe('server auth initialization', () => {
	it('retries after provider discovery becomes available', async () => {
		const unavailableAuth = { $context: Promise.reject(new Error('discovery unavailable')) }
		const initializedAuth = { $context: Promise.resolve() }

		authMocks.createAuth.mockReturnValueOnce(unavailableAuth).mockReturnValueOnce(initializedAuth)

		await expect(getAuth()).rejects.toThrow('discovery unavailable')
		await expect(getAuth()).resolves.toBe(initializedAuth)
		await expect(getAuth()).resolves.toBe(initializedAuth)
		expect(authMocks.createAuth).toHaveBeenCalledTimes(2)
	})
})
