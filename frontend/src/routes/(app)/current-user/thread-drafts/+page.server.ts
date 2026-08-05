import { type ThreadDto, getThreadsPaged, getThreadsCount, ThreadState } from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import { createPagination } from '$lib/utils/value-object'
import { error } from '@sveltejs/kit'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ url, locals }) => {
	const auth = locals.accessToken
	const userId = locals.userId

	if (!auth || !userId) error(401, 'Unauthorized')

	const threadDraftsCount = (
		await getThreadsCount<true>({
			query: { createdBy: userId, status: ThreadState.DRAFT },
			auth
		})
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
			await getThreadsPaged<true>({
				query: {
					...pagination,
					createdBy: userId,
					status: ThreadState.DRAFT
				},
				auth
			})
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
