import { withApiLocale } from '$lib/client/api-options'
import { paraglideMiddleware } from '$lib/paraglide/server'
import { createClient } from '$lib/utils/client/client'
import { getForumsBulk, getForumsCount } from '$lib/utils/client/sdk.gen'
import type { ForumId, UserId } from '$lib/utils/client/types.gen'
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

	it('transforms dates inside bulk Result values from the generated OpenAPI union', async () => {
		const forumId = '019fe7bb-5c9d-7ccb-8dc6-874d81bb18e1' as ForumId
		const createdBy = '4167f9fe-8ec5-4ec2-a4a1-1ec32f5c89d8' as UserId
		const createdAt = '2026-08-11T12:34:56Z'
		const client = createClient({
			baseUrl: 'https://api.example.test',
			fetch: async () =>
				new Response(
					JSON.stringify({
						[forumId]: {
							value: { forumId, title: 'Forum', createdBy, createdAt }
						}
					}),
					{ headers: { 'Content-Type': 'application/json' } }
				)
		})

		const result = await getForumsBulk<true>(
			withApiLocale({
				client,
				path: { forumIds: [forumId] },
				throwOnError: true
			})
		)
		const entry = result.data?.[forumId]

		expect(entry).toBeDefined()
		expect(entry && 'value' in entry && entry.value.createdAt).toEqual(new Date(createdAt))
	})
})
