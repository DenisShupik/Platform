import { withApiLocale } from '$lib/client/api-options'
import { noForumAllowedActions } from '$lib/category-authorization'
import type { CategoryDto, CategoryId, Count, PostDto, UserDto, UserId } from '$lib/utils/client'
import {
	getCategoriesPostsLatest,
	getForumsCategoriesCount,
	getCategoriesPaged,
	getCategoriesPostsCount,
	getCategoriesThreadsCount,
	getForum,
	getForumAllowedActions,
	getUsersBulk
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import { createPagination, zeroCount } from '$lib/utils/value-object'
import { typedEntries } from '$lib/utils/typed-entries'
import { getResultValue, getSuccessfulResultMap } from '$lib/utils/result'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ params, url, locals }) => {
	const auth = locals.accessToken

	const forumId = params.forumId
	const allowedActions = auth
		? (
				await getForumAllowedActions<true>(
					withApiLocale({ path: { forumId }, auth, throwOnError: true })
				)
			).data
		: noForumAllowedActions
	const canCreateCategory = allowedActions.canManageStructure

	const forum = (
		await getForum<true>(
			withApiLocale({
				path: { forumId },
				auth,
				throwOnError: true
			})
		)
	).data

	const categoryCount =
		getResultValue(
			(
				await getForumsCategoriesCount<true>(
					withApiLocale({
						path: { forumIds: [forumId] },
						auth,
						throwOnError: true
					})
				)
			).data[forumId]
		) ?? zeroCount

	const currentPage = getPageFromUrl(url)
	const perPage = 10

	let forumData:
		| {
				forumCategories: CategoryDto[]
				categoryThreadsCount: Map<CategoryId, Count>
				categoryPostsCount: Map<CategoryId, Count>
				categoryLatestPosts: Map<CategoryId, PostDto>
				users: Map<UserId, UserDto>
		  }
		| undefined
	if (categoryCount !== 0) {
		const pagination = createPagination(currentPage, perPage)
		const forumCategories = (
			await getCategoriesPaged<true>(
				withApiLocale({
					query: {
						forumIds: [forumId],
						...pagination
					},
					auth,
					throwOnError: true
				})
			)
		).data

		const categoryIds = forumCategories.map((category) => category.categoryId)

		let categoryThreadsCount: Map<CategoryId, Count>
		if (categoryIds.length > 0) {
			const response = await getCategoriesThreadsCount<true>(
				withApiLocale({
					path: { categoryIds },
					auth,
					throwOnError: true
				})
			)
			categoryThreadsCount = getSuccessfulResultMap(response.data)
		} else {
			categoryThreadsCount = new Map()
		}

		let categoryPostsCount: Map<CategoryId, Count>
		if (categoryIds.length > 0) {
			const response = await getCategoriesPostsCount<true>(
				withApiLocale({
					path: { categoryIds },
					auth,
					throwOnError: true
				})
			)
			categoryPostsCount = getSuccessfulResultMap(response.data)
		} else {
			categoryPostsCount = new Map()
		}

		let categoryLatestPosts: Map<CategoryId, PostDto>
		if (categoryIds.length > 0) {
			const response = await getCategoriesPostsLatest<true>(
				withApiLocale({
					path: { categoryIds },
					auth,
					throwOnError: true
				})
			)
			categoryLatestPosts = new Map(
				typedEntries(response.data).flatMap(([categoryId, item]) =>
					item === undefined ? [] : [[categoryId, item] as const]
				)
			)
		} else {
			categoryLatestPosts = new Map()
		}

		const userIds = new Set([...categoryLatestPosts.values()].flat().map((post) => post.createdBy))

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

		forumData = {
			forumCategories,
			categoryThreadsCount,
			categoryPostsCount,
			categoryLatestPosts,
			users
		}
	}

	return {
		canCreateCategory,
		forum,
		currentPage,
		perPage,
		categoryCount,
		forumData
	}
}
