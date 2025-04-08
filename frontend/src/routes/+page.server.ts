import {
	getForumCategoriesCount,
	getForums,
	getForumsCategoriesLatestByPost,
	getForumsCount
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
			Object.entries(response.data).map(([k, v]) => [BigInt(k), v])
		)
	}

	return {
		forumsCount: (await getForumsCount<true>()).data,
		forums,
		forumCategoriesCount,
		forumsCategoriesLatestByPost
	}
}
