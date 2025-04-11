import {
	getCategory,
	getForum,
	getPosts,
	getThread,
	getThreadsPostsCount,
	getUsersByIds,
	type ThreadDto
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'

import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ params, url }) => {
	const threadId: ThreadDto['threadId'] = params.threadId

	const thread = (await getThread<true>({ path: { threadId } })).data

	const category = (await getCategory<true>({ path: { categoryId: thread.categoryId } })).data

	const forum = (await getForum<true>({ path: { forumId: category.forumId } })).data

	const postCount = BigInt(
		(await getThreadsPostsCount<true>({ path: { threadIds: [threadId] } })).data[`${threadId}`]
	)

	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10n
	const threadPosts = (
		await getPosts<true>({
			query: {
				threadId,
				offset: (currentPage - 1n) * perPage,
				limit: perPage
			}
		})
	).data

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
		currentPage,
		perPage,
		postCount,
		category,
		forum,
		threadPosts,
		users
	}
}
