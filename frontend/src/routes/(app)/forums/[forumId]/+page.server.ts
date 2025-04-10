import {
	getCategoriesLatestPost,
	getForumCategoriesCount,
	type CategoryId,
	type PostDto
} from '$lib/utils/client'
import {
	getCategories,
	getCategoryPostsCount,
	getCategoryThreadsCount,
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
		(await getForumCategoriesCount<true>({ path: { forumIds: [forumId] } })).data[`${forumId}`]
	)

	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10n

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

	let categoryThreadsCount
	if (categoryIds.length > 0) {
		const stats = await getCategoryThreadsCount<true>({
			path: { categoryIds }
		})
		categoryThreadsCount = new Map(Object.entries(stats.data))
	} else {
		categoryThreadsCount = new Map()
	}

	let categoryPostsCount
	if (categoryIds.length > 0) {
		const stats = await getCategoryPostsCount<true>({ path: { categoryIds } })
		categoryPostsCount = new Map(Object.entries(stats.data))
	} else {
		categoryPostsCount = new Map()
	}

	let categoryLatestPosts: Map<CategoryId, PostDto>
	if (categoryIds.length > 0) {
		const response = await getCategoriesLatestPost<true>({
			path: { categoryIds }
		})
		categoryLatestPosts = new Map(Object.entries(response.data))
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
		currentPage,
		perPage,
		categoryCount,
		forumCategories,
		categoryThreadsCount,
		categoryPostsCount,
		categoryLatestPosts,
		users
	}
}
