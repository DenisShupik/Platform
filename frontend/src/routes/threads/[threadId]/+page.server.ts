import {
	getCategory,
	getForum,
	getThread,
	getThreadPosts,
	getThreadPostsCount,
	getUsersByIds,
	type ThreadDto
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'

import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ params, url }) => {
	const threadId: ThreadDto['threadId'] = BigInt(params.threadId)

	const thread = (await getThread<true>({ path: { threadId } })).data

	const category = (await getCategory<true>({ path: { categoryId: thread.categoryId } })).data

	const forum = (await getForum<true>({ path: { forumId: category.forumId } })).data

	const postCount = BigInt(
		(await getThreadPostsCount<true>({ path: { threadIds: [threadId] } })).data[`${threadId}`]
	)

	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10n
	const pageIndex = (currentPage - 1n) * perPage
	const threadPosts = (
		await getThreadPosts<true>({
			path: { threadId },
			query: {
				cursor: pageIndex,
				limit: perPage
			}
		})
	).data.items

	const userIds = new Set(threadPosts.map((post) => post.createdBy))

	let users
	if (userIds.size > 0) {
		const response = await getUsersByIds<true>({ path: { userIds: [...userIds] } })
		users = new Map(response.data.map((item) => [item.userId, item]))
	} else {
		users = new Map()
	}

	return {
		thread,
		pageIndex,
		perPage,
		postCount,
		category,
		forum,
		threadPosts,
		users
	}
}
