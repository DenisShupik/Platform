import {
	getCategory,
	getCategoryThreadsPaged,
	getCategoriesThreadsCount,
	getForum,
	getThreadsPostsCount,
	getThreadsPostsLatest,
	getUsersBulk,
	type CategoryDto,
	type PostDto,
	type ThreadId,
	type ThreadDto,
	type UserId,
	type UserDto,
	GetCategoryThreadsPagedQuerySortType
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'

import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ params, url }) => {
	const categoryId: CategoryDto['categoryId'] = params.categoryId

	const category = (await getCategory<true>({ path: { categoryId } })).data

	const categoryThreadsCount =
		(await getCategoriesThreadsCount<true>({ path: { categoryIds: [categoryId] } })).data[
			`${categoryId}`
		] ?? 0n

	const forum = (await getForum<true>({ path: { forumId: category.forumId } })).data

	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10n

	let categoryData:
		| {
				categoryThreads: ThreadDto[]
				threadsPostsLatest: Map<ThreadId, PostDto>
				threadsPostsCount: Map<ThreadId, bigint>
				users: Map<UserId, UserDto>
		  }
		| undefined

	if (categoryThreadsCount !== 0n) {
		const categoryThreads = (
			await getCategoryThreadsPaged<true>({
				path: { categoryId },
				query: {
					offset: (currentPage - 1n) * perPage,
					limit: perPage,
					sort: GetCategoryThreadsPagedQuerySortType.ACTIVITY_DESC
				}
			})
		).data

		const threadIds = categoryThreads.map((thread) => thread.threadId)

		let threadsPostsLatest: Map<ThreadId, PostDto>
		if (threadIds.length > 0) {
			const response = await getThreadsPostsLatest<true>({
				path: { threadIds }
			})
			threadsPostsLatest = new Map(Object.entries(response.data))
		} else {
			threadsPostsLatest = new Map()
		}

		let threadsPostsCount: Map<ThreadId, bigint>
		if (threadIds.length > 0) {
			const response = await getThreadsPostsCount<true>({
				path: { threadIds }
			})
			threadsPostsCount = new Map(Object.entries(response.data))
		} else {
			threadsPostsCount = new Map()
		}

		const userIds = new Set(categoryThreads.map((thread) => thread.createdBy))
		threadsPostsLatest.values().forEach((post) => userIds.add(post.createdBy))

		let users
		if (userIds.size > 0) {
			const response = await getUsersBulk<true>({ path: { userIds: [...userIds] } })
			users = new Map(response.data.map((item) => [item.userId, item]))
		} else {
			users = new Map()
		}

		categoryData = {
			categoryThreads,
			threadsPostsLatest,
			threadsPostsCount,
			users
		}
	}

	return {
		category,
		currentPage,
		perPage,
		categoryThreadsCount,
		forum,
		categoryData
	}
}
