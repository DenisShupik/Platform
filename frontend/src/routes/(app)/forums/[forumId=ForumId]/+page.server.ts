import type { CategoryDto, CategoryId, ForumId, PostDto, UserDto, UserId } from '$lib/utils/client'
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
	const session = await locals.auth()
	const auth = session?.access_token

	const forumId: ForumId = params.forumId

	const forum = (
		await getForum<true>({
			path: { forumId },
			auth
		})
	).data

	const categoryCount = BigInt(
		(
			await getForumsCategoriesCount<true>({
				path: { forumIds: [forumId] },
				auth
			})
		).data[`${forumId}`] ?? 0
	)

	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10n

	let forumData:
		| {
				forumCategories: CategoryDto[]
				categoryThreadsCount: Map<CategoryId, bigint>
				categoryPostsCount: Map<CategoryId, bigint>
				categoryLatestPosts: Map<CategoryId, PostDto>
				users: Map<UserId, UserDto>
		  }
		| undefined
	if (categoryCount !== 0n) {
		const forumCategories = (
			await getCategoriesPaged<true>({
				query: {
					forumIds: [forumId],
					offset: (currentPage - 1n) * perPage,
					limit: perPage
				},
				auth
			})
		).data

		const categoryIds = forumCategories.map((category) => category.categoryId)

		let categoryThreadsCount: Map<CategoryId, bigint>
		if (categoryIds.length > 0) {
			const response = await getCategoriesThreadsCount<true>({
				path: { categoryIds },
				auth
			})
			categoryThreadsCount = new Map(Object.entries(response.data))
		} else {
			categoryThreadsCount = new Map()
		}

		let categoryPostsCount: Map<CategoryId, bigint>
		if (categoryIds.length > 0) {
			const response = await getCategoriesPostsCount<true>({
				path: { categoryIds },
				auth
			})
			categoryPostsCount = new Map(Object.entries(response.data))
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
			users = new Map(response.data.map((item) => [item.userId, item]))
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
		forum,
		currentPage,
		perPage,
		categoryCount,
		forumData
	}
}
