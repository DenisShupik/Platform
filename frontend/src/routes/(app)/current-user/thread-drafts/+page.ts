import { type ThreadDto, getThreads, getThreadsCount } from '$lib/utils/client'
import { authStore, currentUser } from '$lib/client/auth-state.svelte'
import { get } from 'svelte/store'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import type { PageLoad } from './$types'

export const ssr = false
export const csr = true

export const load: PageLoad = async ({ url }) => {
	const userId = get(currentUser)?.id
	if (!userId) {
		await get(authStore).login()
	}

	const threadDraftsCount = BigInt(
		(
			await getThreadsCount<true>({
				query: { createdBy: userId, status: 0 },
				auth: get(authStore).token
			})
		).data
	)

	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10n

	let extraData:
		| {
				threadDrafts: ThreadDto[]
		  }
		| undefined

	if (threadDraftsCount !== 0n) {
		const threadDrafts = (
			await getThreads<true>({
				query: {
					offset: (currentPage - 1n) * perPage,
					limit: perPage,
					createdBy: userId,
					status: 0
				},
				auth: get(authStore).token
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
