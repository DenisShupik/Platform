import { getThreadSubscriptionsPaged } from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import { createPagination } from '$lib/utils/value-object'
import { error } from '@sveltejs/kit'
import type { PageServerLoad } from './$types'

const perPage = 10

export const load: PageServerLoad = async ({ url, locals }) => {
	const auth = locals.accessToken
	const userId = locals.userId
	if (!auth || !userId) error(401, 'Unauthorized')

	const currentPage = getPageFromUrl(url)
	const threadSubscriptionsData = (
		await getThreadSubscriptionsPaged<true>({
			path: { userId },
			query: createPagination(currentPage, perPage),
			auth
		})
	).data

	return { currentPage, perPage, threadSubscriptionsData }
}
