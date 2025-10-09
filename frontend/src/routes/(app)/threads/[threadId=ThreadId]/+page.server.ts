import {
	getCategory,
	getForum,
	getThreadPostsPaged,
	getThread,
	getThreadsPostsCount,
	getUsersBulk,
	type PostDto,
	type ThreadId,
	type UserDto,
	type UserId,
	getThreadSubscriptionStatus
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'

import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ params, url, locals }) => {
	const session = await locals.auth()
	const auth = session?.access_token

	const threadId: ThreadId = params.threadId

	const thread = (
		await getThread<true>({
			path: { threadId },
			auth
		})
	).data

	const category = (
		await getCategory<true>({
			path: { categoryId: thread.categoryId },
			auth
		})
	).data

	const forum = (
		await getForum<true>({
			path: { forumId: category.forumId },
			auth
		})
	).data

	const postCount =
		(
			await getThreadsPostsCount<true>({
				path: { threadIds: [threadId] },
				auth
			})
		).data[`${threadId}`] ?? 0n

	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10n

	let threadData: { threadPosts: PostDto[]; users: Map<UserId, UserDto> } | undefined

	if (postCount !== 0n) {
		const threadPosts = (
			await getThreadPostsPaged<true>({
				path: { threadId },
				query: {
					offset: (currentPage - 1n) * perPage,
					limit: perPage
				},
				auth
			})
		).data

		const userIds = new Set(threadPosts.map((post) => post.createdBy))

		let users
		if (userIds.size > 0) {
			const response = await getUsersBulk<true>({ path: { userIds: [...userIds] } })
			users = new Map(response.data.map((item) => [item.userId, item]))
		} else {
			users = new Map()
		}
		threadData = { threadPosts, users }
	}

	const isSubscribed = session
		? (
				await getThreadSubscriptionStatus<true>({
					path: { threadId },
					auth
				})
			).data.isSubscribed
		: undefined

	return {
		thread,
		category,
		forum,
		currentPage,
		perPage,
		postCount,
		threadData,
		isSubscribed
	}
}
