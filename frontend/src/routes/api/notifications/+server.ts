import type { RequestHandler } from './$types'
import {
	getInternalNotificationsPaged,
	GetInternalNotificationsPagedQuerySortType
} from '$lib/utils/client'
import { error } from '@sveltejs/kit'

export const GET: RequestHandler = async (event) => {
	const session = await event.locals.auth()
	const auth = session?.access_token

	if (auth) {
		const result = await getInternalNotificationsPaged({
			query: {
				isDelivered: false,
				sort: [GetInternalNotificationsPagedQuerySortType.OCCURRED_AT_ASC]
			},
			auth
		})

		return new Response(JSON.stringify(result.data), {
			headers: { 'Content-Type': 'application/json' }
		})
	} else {
		error(401, 'Unauthorized')
	}
}
