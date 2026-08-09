import { withApiLocale } from '$lib/client/api-options'
import { canCreateCategoryPolicy } from '$lib/roles'
import type { CategoryDto, CategoryId, Count, PostDto, UserDto, UserId } from '$lib/utils/client'
import {
	getCategoriesPostsLatest,
	getForumsCategoriesCount,
	getCategoriesPaged,
	getCategoriesPostsCount,
	getCategoriesThreadsCount,
	getForum,
	getUsersBulk
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import { createPagination, zeroCount } from '$lib/utils/value-object'
import { typedEntries } from '$lib/utils/typed-entries'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ params, url, locals }) => {
	const auth = locals.accessToken

	const canCreateCategory = canCreateCategoryPolicy(locals.role)

	const forumId = params.forumId

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
		(
			await getForumsCategoriesCount<true>(
				withApiLocale({
					path: { forumIds: [forumId] },
					auth,
					throwOnError: true
				})
			)
		).data[forumId]?.value ?? zeroCount

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
			categoryThreadsCount = new Map(
				typedEntries(response.data).flatMap(([categoryId, item]) =>
					item?.value == null ? [] : [[categoryId, item.value] as const]
				)
			)
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
			categoryPostsCount = new Map(
				typedEntries(response.data).flatMap(([categoryId, item]) =>
					item?.value == null ? [] : [[categoryId, item.value] as const]
				)
			)
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
			users = new Map(
				typedEntries(response.data).flatMap(([userId, item]) =>
					item?.value == null ? [] : [[userId, item.value] as const]
				)
			)
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
