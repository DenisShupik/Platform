import {
	getWatchedThreadsCount,
	getWatchedThreadsPaged,
	getThreadsBulk,
	type ThreadDto,
	type WatchedThreadDto
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import { typedEntries } from '$lib/utils/typed-entries'
import { createPagination } from '$lib/utils/value-object'
import { error } from '@sveltejs/kit'
import type { PageServerLoad } from './$types'

const perPage = 10

export const load: PageServerLoad = async ({ url, locals }) => {
	const auth = locals.accessToken
	if (!auth) error(401, 'Unauthorized')

	const watchedThreadsCount = (await getWatchedThreadsCount<true>({ auth })).data
	const currentPage = getPageFromUrl(url)

	let watchedThreads: ThreadDto[] | undefined

	if (watchedThreadsCount !== 0) {
		const watchedThreadDtos = (
			await getWatchedThreadsPaged<true>({
				query: createPagination(currentPage, perPage),
				auth
			})
		).data

		watchedThreads = await getWatchedThreads(watchedThreadDtos, auth)
	}

	return { watchedThreadsCount, currentPage, perPage, watchedThreads }
}

async function getWatchedThreads(watchedThreadDtos: WatchedThreadDto[], auth: string) {
	if (watchedThreadDtos.length === 0) return []

	const threadIds = watchedThreadDtos.map((watchedThread) => watchedThread.threadId)
	const threadsResponse = await getThreadsBulk<true>({ path: { threadIds }, auth })
	const threads = new Map(
		typedEntries(threadsResponse.data).flatMap(([threadId, item]) =>
			item?.value == null ? [] : [[threadId, item.value] as const]
		)
	)

	return threadIds.flatMap((threadId) => {
		const thread = threads.get(threadId)
		return thread ? [thread] : []
	})
}
