import { canCreateThreadPolicy } from '$lib/roles'
import {
	getCategory,
	getCategoryThreadsPaged,
	getCategoriesThreadsCount,
	getForum,
	getThreadsPostsCount,
	getThreadsPostsLatest,
	getUsersBulk,
	type PostDto,
	type ThreadId,
	type ThreadDto,
	type UserId,
	type UserDto,
	GetCategoryThreadsPagedQuerySortType,
	type CategoryId,
	type Count
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ params, url, locals }) => {
	const auth = locals.accessToken

	const canCreateThread = canCreateThreadPolicy(locals.role)

	const categoryId: CategoryId = params.categoryId

	const category = (
		await getCategory<true>({
			path: { categoryId },
			auth
		})
	).data

	const categoryThreadsCount =
		(
			await getCategoriesThreadsCount<true>({
				path: { categoryIds: [categoryId] },
				auth
			})
		).data[`${categoryId}`]?.value ?? 0

	const forum = (
		await getForum<true>({
			path: { forumId: category.forumId },
			auth
		})
	).data

	const currentPage = getPageFromUrl(url)
	const perPage = 10

	let categoryData:
		| {
				categoryThreads: ThreadDto[]
				threadsPostsLatest: Map<ThreadId, PostDto | undefined>
				threadsPostsCount: Map<ThreadId, Count | undefined>
				users: Map<UserId, UserDto>
		  }
		| undefined

	if (categoryThreadsCount !== 0) {
		const categoryThreads = (
			await getCategoryThreadsPaged<true>({
				path: { categoryId },
				query: {
					offset: (currentPage - 1) * perPage,
					limit: perPage,
					sort: GetCategoryThreadsPagedQuerySortType.ACTIVITY_DESC
				},
				auth
			})
		).data

		const threadIds = categoryThreads.map((thread) => thread.threadId)

		let threadsPostsLatest: Map<ThreadId, PostDto | undefined>
		if (threadIds.length > 0) {
			const response = await getThreadsPostsLatest<true>({
				path: { threadIds },
				auth
			})
			threadsPostsLatest = new Map(
				Object.entries(response.data).map(([threadId, item]) => [threadId, item.value])
			)
		} else {
			threadsPostsLatest = new Map()
		}

		let threadsPostsCount: Map<ThreadId, Count>
		if (threadIds.length > 0) {
			const response = await getThreadsPostsCount<true>({
				path: { threadIds },
				auth
			})
			threadsPostsCount = new Map(
				Object.entries(response.data)
					.filter(([, item]) => item.value != null)
					.map(([key, item]) => [key, item.value!])
			)
		} else {
			threadsPostsCount = new Map()
		}

		const userIds = new Set(categoryThreads.map((thread) => thread.createdBy))
		threadsPostsLatest.values().forEach((post) => {
			if (post != null) userIds.add(post.createdBy)
		})

		let users
		if (userIds.size > 0) {
			const response = await getUsersBulk<true>({
				path: { userIds: [...userIds] }
			})
			users = new Map(
				Object.entries(response.data)
					.filter(([, item]) => item.value != null)
					.map(([key, item]) => [key, item.value!])
			)
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
		canCreateThread,
		category,
		currentPage,
		perPage,
		categoryThreadsCount,
		forum,
		categoryData
	}
}
