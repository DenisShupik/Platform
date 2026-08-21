import { withApiLocale } from '$lib/client/api-options'
import { noCategoryAllowedActions } from '$lib/category-authorization'
import {
	getCategory,
	getCategoryAllowedActions,
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
	type Count
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import { createPagination, zeroCount } from '$lib/utils/value-object'
import { getResultValue, getSuccessfulResultMap } from '$lib/utils/result'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ params, url, locals }) => {
	const auth = locals.accessToken

	const canCreateThread = locals.userId !== undefined

	const categoryId = params.categoryId

	const category = (
		await getCategory<true>(
			withApiLocale({
				path: { categoryId },
				auth,
				throwOnError: true
			})
		)
	).data

	const categoryThreadsCount =
		getResultValue(
			(
				await getCategoriesThreadsCount<true>(
					withApiLocale({
						path: { categoryIds: [categoryId] },
						auth,
						throwOnError: true
					})
				)
			).data[categoryId]
		) ?? zeroCount

	const forum = (
		await getForum<true>(
			withApiLocale({
				path: { forumId: category.forumId },
				auth,
				throwOnError: true
			})
		)
	).data

	const allowedActions = auth
		? (
				await getCategoryAllowedActions<true>(
					withApiLocale({
						path: { categoryId },
						auth,
						throwOnError: true
					})
				)
			).data
		: noCategoryAllowedActions

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
		const pagination = createPagination(currentPage, perPage)
		const categoryThreads = (
			await getCategoryThreadsPaged<true>(
				withApiLocale({
					path: { categoryId },
					query: {
						...pagination,
						sort: GetCategoryThreadsPagedQuerySortType.ACTIVITY_DESC
					},
					auth,
					throwOnError: true
				})
			)
		).data

		const threadIds = categoryThreads.map((thread) => thread.threadId)

		let threadsPostsLatest: Map<ThreadId, PostDto | undefined>
		if (threadIds.length > 0) {
			const response = await getThreadsPostsLatest<true>(
				withApiLocale({
					path: { threadIds },
					auth,
					throwOnError: true
				})
			)
			threadsPostsLatest = getSuccessfulResultMap(response.data)
		} else {
			threadsPostsLatest = new Map()
		}

		let threadsPostsCount: Map<ThreadId, Count>
		if (threadIds.length > 0) {
			const response = await getThreadsPostsCount<true>(
				withApiLocale({
					path: { threadIds },
					auth,
					throwOnError: true
				})
			)
			threadsPostsCount = getSuccessfulResultMap(response.data)
		} else {
			threadsPostsCount = new Map()
		}

		const userIds = new Set(categoryThreads.map((thread) => thread.createdBy))
		threadsPostsLatest.values().forEach((post) => {
			if (post != null) userIds.add(post.createdBy)
		})

		let users: Map<UserId, UserDto>
		if (userIds.size > 0) {
			const response = await getUsersBulk<true>(
				withApiLocale({
					path: { userIds: [...userIds] },
					throwOnError: true
				})
			)
			users = getSuccessfulResultMap(response.data)
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
		allowedActions,
		category,
		currentPage,
		perPage,
		categoryThreadsCount,
		forum,
		categoryData
	}
}
