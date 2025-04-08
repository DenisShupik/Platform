import {
	getCategoryPosts,
	getCategoryPostsCount,
	getCategoryThreadsCount,
	getForumCategoriesCount,
	getForums,
	getForumsCategoriesLatestByPost,
	getForumsCount,
	getThreadPostsCount,
	getThreadPostsLatest,
	getUsers,
	getUsersByIds
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ url }) => {
	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10

	const forums = (
		await getForums<true>({
			query: {
				cursor: (currentPage - 1n) * BigInt(perPage),
				limit: perPage,
				sort: '-latestPost'
			}
		})
	).data.items

	const forumIds = forums.map((forum) => forum.forumId)
	let forumCategoriesCount
	{
		const response = await getForumCategoriesCount<true>({ path: { forumIds } })
		forumCategoriesCount = new Map(Object.entries(response.data).map(([k, v]) => [BigInt(k), v]))
	}

	let forumsCategoriesLatestByPost
	{
		const response = await getForumsCategoriesLatestByPost<true>({
			path: { forumIds }
		})
		forumsCategoriesLatestByPost = new Map(
			Object.entries(response.data).map(([k, v]) => {
				v.forEach((category) => {
					category.categoryId = BigInt(category.categoryId)
				})
				return [BigInt(k), v]
			})
		)
	}

	const categoryIds = [...forumsCategoriesLatestByPost.values()]
		.flat()
		.map((category) => category.categoryId)

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

	const userIds = [...categoryLatestPosts.values()].flat().map((post) => post.createdBy)

	let users
	if (userIds.length > 0) {
		const response = await getUsersByIds<true>({ path: { userIds } })
		users = new Map(response.data.map((item) => [item.userId, item]))
	} else {
		users = new Map()
	}
	
	// const threadIds = [...forumsCategoriesLatestByPost.values()]
	// 	.flat()
	// 	.flatMap((category) => category.threads.map((thread) => thread.threadId))

	// let threadPostsLatest
	// if (threadIds.length > 0) {
	// 	const response = await getThreadPostsLatest<true>({
	// 		path: { threadIds }
	// 	})
	// 	threadPostsLatest = new Map(response.data.map((item) => [item.threadId, item]))
	// } else {
	// 	threadPostsLatest = new Map()
	// }

	// let threadPostsCount
	// if (threadIds.length > 0) {
	// 	const response = await getThreadPostsCount<true>({
	// 		path: { threadIds }
	// 	})
	// 	threadPostsCount = new Map(Object.entries(response.data).map(([k, v]) => [BigInt(k), v]))
	// } else {
	// 	threadPostsCount = new Map()
	// }

	return {
		forumsCount: (await getForumsCount<true>()).data,
		forums,
		forumCategoriesCount,
		forumsCategoriesLatestByPost,
		categoryThreadsCount,
		categoryPostsCount,
		categoryLatestPosts,
		users
	}
}
