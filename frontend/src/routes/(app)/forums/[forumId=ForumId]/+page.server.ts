import {
	getCategoriesPostsLatest,
	getForumsCategoriesCount,
	type CategoryDto,
	type CategoryId,
	type PostDto,
	type UserDto,
	type UserId
} from '$lib/utils/client'
import {
	getCategories,
	getCategoriesPostsCount,
	getCategoriesThreadsCount,
	getForum,
	getUsersByIds,
	type ForumDto
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ params, url }) => {
	const forumId: ForumDto['forumId'] = params.forumId

	const forum = (await getForum<true>({ path: { forumId } })).data

	const categoryCount = BigInt(
		(await getForumsCategoriesCount<true>({ path: { forumIds: [forumId] } })).data[`${forumId}`] ??
			0
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
			await getCategories<true>({
				query: {
					forumId,
					offset: (currentPage - 1n) * perPage,
					limit: perPage
				}
			})
		).data

		const categoryIds = forumCategories.map((category) => category.categoryId)

		let categoryThreadsCount: Map<CategoryId, bigint>
		if (categoryIds.length > 0) {
			const response = await getCategoriesThreadsCount<true>({
				path: { categoryIds }
			})
			categoryThreadsCount = new Map(Object.entries(response.data))
		} else {
			categoryThreadsCount = new Map()
		}

		let categoryPostsCount: Map<CategoryId, bigint>
		if (categoryIds.length > 0) {
			const response = await getCategoriesPostsCount<true>({ path: { categoryIds } })
			categoryPostsCount = new Map(Object.entries(response.data))
		} else {
			categoryPostsCount = new Map()
		}

		let categoryLatestPosts: Map<CategoryId, PostDto>
		if (categoryIds.length > 0) {
			const response = await getCategoriesPostsLatest<true>({
				path: { categoryIds }
			})
			categoryLatestPosts = new Map(Object.entries(response.data))
		} else {
			categoryLatestPosts = new Map()
		}

		const userIds = new Set([...categoryLatestPosts.values()].flat().map((post) => post.createdBy))

		let users: Map<UserId, UserDto>
		if (userIds.size > 0) {
			const response = await getUsersByIds<true>({ path: { userIds: [...userIds] } })
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
