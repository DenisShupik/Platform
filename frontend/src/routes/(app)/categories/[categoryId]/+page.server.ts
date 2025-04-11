import {
	getCategory,
	getCategoryThreads,
	getCategoriesThreadsCount,
	getForum,
	getThreadsPostsCount,
	getThreadsPostsLatest,
	getUsersByIds,
	type CategoryDto,
	type PostDto,
	type ThreadId
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'

import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ params, url }) => {
	const categoryId: CategoryDto['categoryId'] = params.categoryId

	const category = (await getCategory<true>({ path: { categoryId } })).data

	const categoryThreadsCount = BigInt(
		(await getCategoriesThreadsCount<true>({ path: { categoryIds: [categoryId] } })).data[
			`${categoryId}`
		]
	)

	const forum = (await getForum<true>({ path: { forumId: category.forumId } })).data

	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10n

	const categoryThreads = (
		await getCategoryThreads<true>({
			path: { categoryId },
			query: {
				offset: (currentPage - 1n) * perPage,
				limit: perPage,
				sort: '-Activity'
			}
		})
	).data

	const threadIds = categoryThreads.map((thread) => thread.threadId)

	let threadPostsLatest: Map<ThreadId, PostDto>
	if (threadIds.length > 0) {
		const response = await getThreadsPostsLatest<true>({
			path: { threadIds }
		})
		threadPostsLatest = new Map(Object.entries(response.data))
	} else {
		threadPostsLatest = new Map()
	}

	let threadPostsCount: Map<ThreadId, bigint>
	if (threadIds.length > 0) {
		const response = await getThreadsPostsCount<true>({
			path: { threadIds }
		})
		threadPostsCount = new Map(Object.entries(response.data))
	} else {
		threadPostsCount = new Map()
	}

	const userIds = new Set(categoryThreads.map((thread) => thread.createdBy))
	threadPostsLatest.values().forEach((post) => userIds.add(post.createdBy))

	let users
	if (userIds.size > 0) {
		const response = await getUsersByIds<true>({ path: { userIds: [...userIds] } })
		users = new Map(response.data.map((item) => [item.userId, item]))
	} else {
		users = new Map()
	}

	return {
		category,
		currentPage,
		perPage,
		categoryThreadsCount,
		forum,
		threads: categoryThreads,
		threadPostsLatest,
		threadPostsCount,
		users
	}
}
