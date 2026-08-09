import { withApiLocale } from '$lib/client/api-options'
import { paraglideMiddleware } from '$lib/paraglide/server'
import { createClient } from '$lib/utils/client/client'
import { getForumsCount } from '$lib/utils/client/sdk.gen'
import { describe, expect, it } from 'vitest'

async function withRussianRequest<T>(run: () => Promise<T>): Promise<T> {
	let result: T | undefined
	await paraglideMiddleware(
		new Request('https://example.test/forums', { headers: { Cookie: 'PARAGLIDE_LOCALE=ru' } }),
		async () => {
			result = await run()
			return new Response(null, { status: 204 })
		}
	)
	return result as T
}

describe('generated API error semantics', () => {
	it('honors explicit throwing and non-throwing call-site policies in the client runtime', async () => {
		const requests: Request[] = []
		const failingFetch: typeof fetch = async (input, init) => {
			requests.push(input instanceof Request ? input : new Request(input, init))
			return new Response(JSON.stringify({ title: 'Request failed' }), {
				status: 500,
				headers: { 'Content-Type': 'application/problem+json' }
			})
		}
		const clientWithCsrDefaults = createClient({
			baseUrl: 'https://api.example.test',
			fetch: failingFetch
		})
		const clientWithSsrDefaults = createClient({
			baseUrl: 'https://api.example.test',
			fetch: failingFetch,
			throwOnError: true
		})

		await withRussianRequest(async () => {
			await expect(
				getForumsCount<true>(withApiLocale({ client: clientWithCsrDefaults, throwOnError: true }))
			).rejects.toBeDefined()

			const result = await getForumsCount<false>(
				withApiLocale({ client: clientWithSsrDefaults, throwOnError: false })
			)
			expect(result.error).toBeDefined()
			expect(result.response?.status).toBe(500)
		})

		expect(requests).toHaveLength(2)
		for (const request of requests) {
			expect(request.headers.get('Accept-Language')).toBe('ru')
		}
	})
})
