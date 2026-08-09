import { withApiLocale } from '$lib/client/api-options'
import { type ThreadDto, getThreadsPaged, getThreadsCount, ThreadState } from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import { createPagination } from '$lib/utils/value-object'
import { error } from '@sveltejs/kit'
import * as m from '$lib/paraglide/messages'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ url, locals }) => {
	const auth = locals.accessToken
	const userId = locals.userId

	if (!auth || !userId) error(401, m.error_unauthorized())

	const threadDraftsCount = (
		await getThreadsCount<true>(
			withApiLocale({
				query: { createdBy: userId, status: ThreadState.DRAFT },
				auth,
				throwOnError: true
			})
		)
	).data

	const currentPage = getPageFromUrl(url)
	const perPage = 10

	let extraData:
		| {
				threadDrafts: ThreadDto[]
		  }
		| undefined

	if (threadDraftsCount !== 0) {
		const pagination = createPagination(currentPage, perPage)
		const threadDrafts = (
			await getThreadsPaged<true>(
				withApiLocale({
					query: {
						...pagination,
						createdBy: userId,
						status: ThreadState.DRAFT
					},
					auth,
					throwOnError: true
				})
			)
		).data

		extraData = {
			threadDrafts
		}
	}

	return {
		currentPage,
		perPage,
		threadDraftsCount,
		extraData
	}
}
