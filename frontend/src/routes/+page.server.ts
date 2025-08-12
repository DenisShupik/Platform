import {
	ForumContainsFilter,
	getCategoriesPostsLatest,
	getCategoriesPostsCount,
	getCategoriesThreadsCount,
	getForumsCategoriesCount,
	getForums,
	getForumsCategoriesLatest,
	getForumsCount,
	getUsersBulk,
	type CategoryId,
	type PostDto,
	type UserId,
	type UserDto,
	type ForumDto,
	type ForumId,
	type CategoryDto,
	GetForumsQuerySortEnum
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ url }) => {
	const currentPage: bigint = getPageFromUrl(url)
	const contains = getContainsFromUrl(url)
	const perPage = 10n

	const forumsCount = (
		await getForumsCount<true>({ query: { ...(contains !== undefined && { contains }) } })
	).data

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
			await getForums<true>({
				query: {
					offset: (currentPage - 1n) * BigInt(perPage),
					limit: perPage,
					sort: GetForumsQuerySortEnum.LATEST_POST_DESC,
					...(contains !== undefined && { contains })
				}
			})
		).data

		const forumIds = forums.map((forum) => forum.forumId)

		let forumCategoriesCount
		{
			const response = await getForumsCategoriesCount<true>({ path: { forumIds } })
			forumCategoriesCount = new Map(Object.entries(response.data))
		}

		let forumsCategoriesLatest
		{
			const response = await getForumsCategoriesLatest<true>({
				path: { forumIds }
			})
			forumsCategoriesLatest = new Map(Object.entries(response.data))
		}

		const categoryIds = [...forumsCategoriesLatest.values()]
			.flat()
			.map((category) => category.categoryId)

		let categoriesThreadsCount
		if (categoryIds.length > 0) {
			const response = await getCategoriesThreadsCount<true>({
				path: { categoryIds }
			})
			categoriesThreadsCount = new Map(Object.entries(response.data))
		} else {
			categoriesThreadsCount = new Map()
		}

		let categoriesPostsCount
		if (categoryIds.length > 0) {
			const response = await getCategoriesPostsCount<true>({ path: { categoryIds } })
			categoriesPostsCount = new Map(Object.entries(response.data))
		} else {
			categoriesPostsCount = new Map()
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

		const userIds = new Set(
			[...categoriesPostsLatest.values()].flat().map((post) => post.createdBy)
		)

		let users: Map<UserId, UserDto>
		if (userIds.size > 0) {
			const response = await getUsersBulk<true>({ path: { userIds: [...userIds] } })
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
		currentPage,
		perPage,
		forumsCount,
		contains:
			contains === ForumContainsFilter.CATEGORY
				? 'categories'
				: contains === ForumContainsFilter.THREAD
					? 'threads'
					: contains === ForumContainsFilter.POST
						? ''
						: 'all',
		forumsData
	}
}

function getContainsFromUrl(url: URL): ForumContainsFilter | undefined {
	const param = url.searchParams.get('contains')
	switch (param) {
		case 'all':
			return undefined
		case 'categories':
			return ForumContainsFilter.CATEGORY
		case 'threads':
			return ForumContainsFilter.THREAD
		default:
			return ForumContainsFilter.POST
	}
}
