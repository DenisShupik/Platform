import { type ThreadDto, getThreadsPaged, getThreadsCount } from '$lib/utils/client'
import { currentUser, login } from '$lib/client/current-user-state.svelte'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import type { PageLoad } from './$types'

export const ssr = false
export const csr = true

export const load: PageLoad = async ({ url, fetch }) => {
	const userId = currentUser.user?.id
	if (!userId) {
		await login()
	}

	const threadDraftsCount = (
		await getThreadsCount<true>({
			query: { createdBy: userId, status: 0 },
			auth: currentUser.user?.token,
			fetch
		})
	).data

	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10n

	let extraData:
		| {
				threadDrafts: ThreadDto[]
		  }
		| undefined

	if (threadDraftsCount !== 0n) {
		const threadDrafts = (
			await getThreadsPaged<true>({
				query: {
					offset: (currentPage - 1n) * perPage,
					limit: perPage,
					createdBy: userId,
					status: 0
				},
				auth: currentUser.user?.token
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
