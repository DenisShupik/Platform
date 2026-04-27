import { canCreateCategoryPolicy } from '$lib/roles'
import type {
	CategoryDto,
	CategoryId,
	Count,
	ForumId,
	PostDto,
	UserDto,
	UserId
} from '$lib/utils/client'
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
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ params, url, locals }) => {
	const auth = locals.accessToken

	const canCreateCategory = canCreateCategoryPolicy(locals.role)

	const forumId: ForumId = params.forumId

	const forum = (
		await getForum<true>({
			path: { forumId },
			auth
		})
	).data

	const categoryCount =
		(
			await getForumsCategoriesCount<true>({
				path: { forumIds: [forumId] },
				auth
			})
		).data[`${forumId}`]?.value ?? 0

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
		const forumCategories = (
			await getCategoriesPaged<true>({
				query: {
					forumIds: [forumId],
					offset: (currentPage - 1) * perPage,
					limit: perPage
				},
				auth
			})
		).data

		const categoryIds = forumCategories.map((category) => category.categoryId)

		let categoryThreadsCount: Map<CategoryId, Count>
		if (categoryIds.length > 0) {
			const response = await getCategoriesThreadsCount<true>({
				path: { categoryIds },
				auth
			})
			categoryThreadsCount = new Map(
				Object.entries(response.data)
					.filter(([, item]) => item.value != null)
					.map(([key, item]) => [key, item.value!])
			)
		} else {
			categoryThreadsCount = new Map()
		}

		let categoryPostsCount: Map<CategoryId, Count>
		if (categoryIds.length > 0) {
			const response = await getCategoriesPostsCount<true>({
				path: { categoryIds },
				auth
			})
			categoryPostsCount = new Map(
				Object.entries(response.data)
					.filter(([, item]) => item.value != null)
					.map(([key, item]) => [key, item.value!])
			)
		} else {
			categoryPostsCount = new Map()
		}

		let categoryLatestPosts: Map<CategoryId, PostDto>
		if (categoryIds.length > 0) {
			const response = await getCategoriesPostsLatest<true>({
				path: { categoryIds },
				auth
			})
			categoryLatestPosts = new Map(Object.entries(response.data))
		} else {
			categoryLatestPosts = new Map()
		}

		const userIds = new Set([...categoryLatestPosts.values()].flat().map((post) => post.createdBy))

		let users: Map<UserId, UserDto>
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
