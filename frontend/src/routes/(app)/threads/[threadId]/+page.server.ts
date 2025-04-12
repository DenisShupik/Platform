import {
	getCategory,
	getForum,
	getPosts,
	getThread,
	getThreadsPostsCount,
	getUsersByIds,
	type PostDto,
	type ThreadId,
	type UserDto,
	type UserId
} from '$lib/utils/client'
import { zThreadId } from '$lib/utils/client/zod.gen'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'

import type { PageServerLoad } from './$types'
import { error } from '@sveltejs/kit'

export const load: PageServerLoad = async ({ params, url }) => {
	const parseResult = zThreadId.safeParse(params.threadId)

	if (!parseResult.success) {
		error(400, {
			message: 'Invalid thread ID',
		});
	}

	const threadId: ThreadId = parseResult.data

	const thread = (await getThread<true>({ path: { threadId } })).data

	const category = (await getCategory<true>({ path: { categoryId: thread.categoryId } })).data

	const forum = (await getForum<true>({ path: { forumId: category.forumId } })).data

	const postCount = BigInt(
		(await getThreadsPostsCount<true>({ path: { threadIds: [threadId] } })).data[`${threadId}`] ??
			0n
	)

	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10n

	let threadData: { threadPosts: PostDto[]; users: Map<UserId, UserDto> } | undefined

	if (postCount !== 0n) {
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
		threadData = { threadPosts, users }
	}

	return {
		thread,
		category,
		forum,
		currentPage,
		perPage,
		postCount,
		threadData
	}
}
