import {
	getCategoriesPostsLatest,
	getCategoriesPostsCount,
	getCategoriesThreadsCount,
	getForumsCategoriesCount,
	getForums,
	getForumsCategoriesLatest,
	getForumsCount,
	getUsersByIds,
	type CategoryId,
	type PostDto
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ url }) => {
	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10n

	const forums = (
		await getForums<true>({
			query: {
				offset: (currentPage - 1n) * BigInt(perPage),
				limit: perPage,
				sort: '-latestPost'
			}
		})
	).data

	const forumIds = forums.map((forum) => forum.forumId)

	let forumCategoriesCount
	{
		const response = await getForumsCategoriesCount<true>({ path: { forumIds } })
		forumCategoriesCount = new Map(Object.entries(response.data))
	}

	let forumsCategoriesLatestByPost
	{
		const response = await getForumsCategoriesLatest<true>({
			path: { forumIds }
		})
		forumsCategoriesLatestByPost = new Map(Object.entries(response.data))
	}

	const categoryIds = [...forumsCategoriesLatestByPost.values()]
		.flat()
		.map((category) => category.categoryId)

	let categoryThreadsCount
	if (categoryIds.length > 0) {
		const response = await getCategoriesThreadsCount<true>({
			path: { categoryIds }
		})
		categoryThreadsCount = new Map(Object.entries(response.data))
	} else {
		categoryThreadsCount = new Map()
	}

	let categoryPostsCount
	if (categoryIds.length > 0) {
		const response = await getCategoriesPostsCount<true>({ path: { categoryIds } })
		categoryPostsCount = new Map(Object.entries(response.data))
	} else {
		categoryPostsCount = new Map()
	}

	let categoriesPostsLatest: Map<CategoryId, PostDto>
	if (categoryIds.length > 0) {
		const response = await getCategoriesPostsLatest<true>({
			path: { categoryIds }
		})
		categoriesPostsLatest = new Map(Object.entries(response.data))
	} else {
		categoriesPostsLatest = new Map()
	}

	const userIds = new Set([...categoriesPostsLatest.values()].flat().map((post) => post.createdBy))

	let users
	if (userIds.size > 0) {
		const response = await getUsersByIds<true>({ path: { userIds: [...userIds] } })
		users = new Map(response.data.map((item) => [item.userId, item]))
	} else {
		users = new Map()
	}

	return {
		forums,
		currentPage,
		perPage,
		forumsCount: (await getForumsCount<true>()).data,
		forumCategoriesCount,
		forumsCategoriesLatestByPost,
		categoryThreadsCount,
		categoryPostsCount,
		categoriesPostsLatest,
		users
	}
}
