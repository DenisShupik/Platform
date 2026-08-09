import { withApiLocale } from '$lib/client/api-options'
import {
	getInternalNotificationsPaged,
	GetInternalNotificationsPagedQuerySortType
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import { createPagination } from '$lib/utils/value-object'
import { error } from '@sveltejs/kit'
import * as m from '$lib/paraglide/messages'
import type { PageServerLoad } from './$types'

const perPage = 10

export const load: PageServerLoad = async ({ url, locals }) => {
	const auth = locals.accessToken
	if (!auth) error(401, m.error_unauthorized())

	const currentPage = getPageFromUrl(url)
	const notificationsData = (
		await getInternalNotificationsPaged<true>(
			withApiLocale({
				query: {
					...createPagination(currentPage, perPage),
					sort: [GetInternalNotificationsPagedQuerySortType.OCCURRED_AT_DESC]
				},
				auth,
				throwOnError: true
			})
		)
	).data

	return { currentPage, perPage, notificationsData }
}
