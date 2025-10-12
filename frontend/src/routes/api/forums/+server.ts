import type { RequestHandler } from './$types'
import { getForumsPaged } from '$lib/utils/client'
import { error } from '@sveltejs/kit'

export const GET: RequestHandler = async ({ url, locals }) => {
	const title = url.searchParams.get('title')

	const session = await locals.auth()
	const auth = session?.access_token
	if (auth) {
		const result = (
			await getForumsPaged<true>({
				query: { title },
				auth
			})
		).data
		return new Response(JSON.stringify(result), { headers: { 'Content-Type': 'application/json' } })
	} else {
		error(401, 'Unauthorized')
	}
}
