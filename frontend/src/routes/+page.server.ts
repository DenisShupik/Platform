import {
	getForumsPaged,
	getCategoriesPostsLatest,
	getCategoriesPostsCount,
	getCategoriesThreadsCount,
	getForumsCategoriesCount,
	getForumsCount,
	getUsersBulk,
	type CategoryId,
	type PostDto,
	type UserId,
	type UserDto,
	type ForumDto,
	type ForumId,
	type CategoryDto,
	getCategoriesPaged,
	getPortalPermissions
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ url, locals }) => {
	const session = await locals.auth()
	const auth = session?.access_token

	const currentPage: bigint = getPageFromUrl(url)
	const perPage = 10n

	const permissions = (await getPortalPermissions<true>({ auth })).data
	const forumsCount = (await getForumsCount<true>({ auth })).data

	let forumsData:
		| {
				forums: ForumDto[]
				forumCategoriesCount: Map<ForumId, bigint>
				forumsCategoriesLatest: Map<ForumId, CategoryDto[]>
				categoriesThreadsCount: Map<CategoryId, bigint>
				categoriesPostsCount: Map<CategoryId, bigint>
				categoriesPostsLatest: Map<CategoryId, PostDto>
				users: Map<UserId, UserDto>
		  }
		| undefined

	if (forumsCount !== 0n) {
		const forums = (
			await getForumsPaged<true>({
				query: {
					offset: (currentPage - 1n) * perPage,
					limit: perPage
				},
				auth
			})
		).data

		const forumIds = forums.map((forum) => forum.forumId)

		let forumCategoriesCount
		{
			const response = await getForumsCategoriesCount<true>({
				path: { forumIds },
				auth
			})
			forumCategoriesCount = new Map(Object.entries(response.data))
		}

		const forumsCategoriesLatest = new Map()
		let categoryIds
		{
			const response = await getCategoriesPaged<true>({
				query: { forumIds },
				auth
			})
			const data = response.data
			categoryIds = new Array(data.length)
			let i = 0
			for (const category of data) {
				const key = category.forumId
				const bucket = forumsCategoriesLatest.get(key)
				if (bucket) bucket.push(category)
				else forumsCategoriesLatest.set(key, [category])
				categoryIds[i++] = category.categoryId
			}
		}

		let categoriesThreadsCount
		if (categoryIds.length > 0) {
			const response = await getCategoriesThreadsCount<true>({
				path: { categoryIds },
				auth
			})
			categoriesThreadsCount = new Map(Object.entries(response.data))
		} else {
			categoriesThreadsCount = new Map()
		}

		let categoriesPostsCount
		if (categoryIds.length > 0) {
			const response = await getCategoriesPostsCount<true>({
				path: { categoryIds },
				auth
			})
			categoriesPostsCount = new Map(Object.entries(response.data))
		} else {
			categoriesPostsCount = new Map()
		}

		let categoriesPostsLatest: Map<CategoryId, PostDto>
		if (categoryIds.length > 0) {
			const response = await getCategoriesPostsLatest<true>({
				path: { categoryIds },
				auth
			})
			categoriesPostsLatest = new Map(Object.entries(response.data))
		} else {
			categoriesPostsLatest = new Map()
		}

		const userIds = new Set(
			[...categoriesPostsLatest.values()].flat().map((post) => post.createdBy)
		)

		let users: Map<UserId, UserDto>
		if (userIds.size > 0) {
			const response = await getUsersBulk<true>({
				path: { userIds: [...userIds] },
				auth
			})
			users = new Map(response.data.map((item) => [item.userId, item]))
		} else {
			users = new Map()
		}

		forumsData = {
			forums,
			forumCategoriesCount,
			forumsCategoriesLatest,
			categoriesThreadsCount,
			categoriesPostsCount,
			categoriesPostsLatest,
			users
		}
	}
	return {
		permissions,
		currentPage,
		perPage,
		forumsCount,
		forumsData
	}
}
