import { canCreateForumPolicy } from '$lib/roles'
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
	type Count
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import {
	createPagination
} from '$lib/utils/value-object'
import { typedEntries } from '$lib/utils/typed-entries'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ url, locals }) => {
	const auth = locals.accessToken

	const currentPage: number = getPageFromUrl(url)
	const perPage = 10

	const canCreateForum = canCreateForumPolicy(locals.role)

	const forumsCount = (await getForumsCount<true>({ auth })).data

	let forumsData:
		| {
				forums: ForumDto[]
				forumCategoriesCount: Map<ForumId, Count>
				forumsCategoriesLatest: Map<ForumId, CategoryDto[]>
				categoriesThreadsCount: Map<CategoryId, Count>
				categoriesPostsCount: Map<CategoryId, Count>
				categoriesPostsLatest: Map<CategoryId, PostDto>
				users: Map<UserId, UserDto>
		  }
		| undefined

	if (forumsCount !== 0) {
		const pagination = createPagination(currentPage, perPage)
		const forums = (
			await getForumsPaged<true>({
				query: {
					...pagination
				},
				auth
			})
		).data

		const forumIds = forums.map((forum) => forum.forumId)

		let forumCategoriesCount: Map<ForumId, Count>
		{
			const response = await getForumsCategoriesCount<true>({
				path: { forumIds },
				auth
			})
			forumCategoriesCount = new Map(
				typedEntries(response.data).flatMap(([forumId, item]) =>
					item?.value == null ? [] : [[forumId, item.value] as const]
				)
			)
		}

		const forumsCategoriesLatest = new Map<ForumId, CategoryDto[]>()
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

		let categoriesThreadsCount: Map<CategoryId, Count>
		if (categoryIds.length > 0) {
			const response = await getCategoriesThreadsCount<true>({
				path: { categoryIds },
				auth
			})
			categoriesThreadsCount = new Map(
				typedEntries(response.data).flatMap(([categoryId, item]) =>
					item?.value == null ? [] : [[categoryId, item.value] as const]
				)
			)
		} else {
			categoriesThreadsCount = new Map()
		}

		let categoriesPostsCount: Map<CategoryId, Count>
		if (categoryIds.length > 0) {
			const response = await getCategoriesPostsCount<true>({
				path: { categoryIds },
				auth
			})
			categoriesPostsCount = new Map(
				typedEntries(response.data).flatMap(([categoryId, item]) =>
					item?.value == null ? [] : [[categoryId, item.value] as const]
				)
			)
		} else {
			categoriesPostsCount = new Map()
		}

		let categoriesPostsLatest: Map<CategoryId, PostDto>
		if (categoryIds.length > 0) {
			const response = await getCategoriesPostsLatest<true>({
				path: { categoryIds },
				auth
			})
			categoriesPostsLatest = new Map(
				typedEntries(response.data).flatMap(([categoryId, item]) =>
					item === undefined ? [] : [[categoryId, item] as const]
				)
			)
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
			users = new Map(
				typedEntries(response.data).flatMap(([userId, item]) =>
					item?.value == null ? [] : [[userId, item.value] as const]
				)
			)
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
		canCreateForum,
		currentPage,
		perPage,
		forumsCount,
		forumsData
	}
}
