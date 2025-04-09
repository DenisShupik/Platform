import {
	getCategoryPosts,
	getCategoryPostsCount,
	getCategoryThreadsCount,
	getForum,
	getForumCategories,
	getUsersByIds,
    type ForumDto
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ params, url }) => {
	const forumId: ForumDto['forumId'] = BigInt(params.forumId)

	const forum = (await getForum<true>({ path: { forumId } })).data

	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10

	const forumCategories = (
		await getForumCategories<true>({
			path: { forumId },
			query: {
				cursor: (currentPage - 1n) * BigInt(perPage),
				limit: perPage
			}
		})
	).data.items

	const categoryIds = forumCategories.map((category) => category.categoryId)

	let categoryThreadsCount
	if (categoryIds.length > 0) {
		const stats = await getCategoryThreadsCount<true>({
			path: { categoryIds }
		})
		categoryThreadsCount = new Map(Object.entries(stats.data).map(([k, v]) => [BigInt(k), v]))
	} else {
		categoryThreadsCount = new Map()
	}

	let categoryPostsCount
	if (categoryIds.length > 0) {
		const stats = await getCategoryPostsCount<true>({ path: { categoryIds } })
		categoryPostsCount = new Map(Object.entries(stats.data).map(([k, v]) => [BigInt(k), v]))
	} else {
		categoryPostsCount = new Map()
	}

	let categoryLatestPosts
	if (categoryIds.length > 0) {
		const response = await getCategoryPosts<true>({
			path: { categoryIds },
			query: { latest: true }
		})
		categoryLatestPosts = new Map(response.data.map((item) => [item.categoryId, item.post]))
	} else {
		categoryLatestPosts = new Map()
	}

	const userIds = new Set([...categoryLatestPosts.values()].flat().map((post) => post.createdBy))

	let users
	if (userIds.size > 0) {
		const response = await getUsersByIds<true>({ path: { userIds: [...userIds] } })
		users = new Map(response.data.map((item) => [item.userId, item]))
	} else {
		users = new Map()
	}

	return {
		forum,
		forumCategories,
		categoryThreadsCount,
		categoryPostsCount,
		categoryLatestPosts,
		users
	}
}
