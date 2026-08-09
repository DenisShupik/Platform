import { getLocale, type Locale } from '$lib/paraglide/runtime'
import { paraglideMiddleware } from '$lib/paraglide/server'
import { withApiLocale } from '$lib/client/api-options'
import { applyLocaleRequestHeader } from '$lib/client/locale-request'
import { describe, expect, it } from 'vitest'

async function withRequestLocale<T>(
	headers: HeadersInit,
	run: (request: Request) => T | Promise<T>,
	pathname = '/forums'
): Promise<T> {
	let result: T | undefined
	await paraglideMiddleware(
		new Request(`https://example.test${pathname}`, { headers }),
		async ({ request }) => {
			result = await run(request)
			return new Response(null, { status: 204 })
		}
	)
	return result as T
}

describe('locale resolution', () => {
	it('uses the explicit locale cookie before the browser language', async () => {
		await expect(
			withRequestLocale(
				{ Cookie: 'PARAGLIDE_LOCALE=ru', 'Accept-Language': 'en-US,en;q=0.9' },
				() => getLocale()
			)
		).resolves.toBe('ru')
	})

	it('uses the preferred browser language when no locale cookie exists', async () => {
		await expect(
			withRequestLocale({ 'Accept-Language': 'ru-RU,ru;q=0.9,en;q=0.8' }, () => getLocale())
		).resolves.toBe('ru')
	})

	it('uses English when neither an explicit nor a supported browser locale exists', async () => {
		await expect(
			withRequestLocale({ 'Accept-Language': 'de-DE' }, () => getLocale())
		).resolves.toBe('en')
	})

	it('keeps paths neutral and does not interpret a prefix as locale state', async () => {
		await expect(
			withRequestLocale(
				{ Cookie: 'PARAGLIDE_LOCALE=en' },
				(request) => [getLocale(), new URL(request.url).pathname],
				'/ru/forums'
			)
		).resolves.toEqual(['en', '/ru/forums'])
	})

	it('isolates locale and propagated headers across concurrent SSR requests', async () => {
		const observe = (locale: Locale, delay: number) =>
			withRequestLocale({ Cookie: `PARAGLIDE_LOCALE=${locale}` }, async () => {
				await new Promise((resolve) => setTimeout(resolve, delay))
				const request = applyLocaleRequestHeader(new Request('https://api.example.test/forums'))
				const options = withApiLocale({
					headers: { 'X-Trace-Id': 'test' },
					throwOnError: true
				})
				return [
					getLocale(),
					request.headers.get('Accept-Language'),
					options.headers['Accept-Language'],
					options.headers['x-trace-id']
				]
			})

		await expect(Promise.all([observe('en', 10), observe('ru', 0)])).resolves.toEqual([
			['en', 'en', 'en', 'test'],
			['ru', 'ru', 'ru', 'test']
		])
	})

	it('excludes Better Auth technical endpoints from locale negotiation', async () => {
		await expect(
			withRequestLocale({ Cookie: 'PARAGLIDE_LOCALE=ru' }, () => getLocale(), '/api/auth/session')
		).resolves.toBe('en')
	})
})
