import {
	getInternalNotificationsPaged,
	GetInternalNotificationsPagedQuerySortType
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import { createPagination } from '$lib/utils/value-object'
import { error } from '@sveltejs/kit'
import type { PageServerLoad } from './$types'

const perPage = 10

export const load: PageServerLoad = async ({ url, locals }) => {
	const auth = locals.accessToken
	if (!auth) error(401, 'Unauthorized')

	const currentPage = getPageFromUrl(url)
	const notificationsData = (
		await getInternalNotificationsPaged<true>({
			query: {
				...createPagination(currentPage, perPage),
				sort: [GetInternalNotificationsPagedQuerySortType.OCCURRED_AT_DESC]
			},
			auth
		})
	).data

	return { currentPage, perPage, notificationsData }
}
