import type { RequestHandler } from './$types'
import { getInternalNotificationCount } from '$lib/utils/client'
import { error } from '@sveltejs/kit'

export const GET: RequestHandler = async (event) => {
	const session = await event.locals.auth()
	const auth = session?.access_token
	if (auth) {
		const result = (
			await getInternalNotificationCount<true>({
				query: { isDelivered: false },
				auth
			})
		).data
		return new Response(JSON.stringify(result), { headers: { 'Content-Type': 'application/json' } })
	} else {
		error(401, 'Unauthorized')
	}
}
