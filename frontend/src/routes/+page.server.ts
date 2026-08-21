import { withApiLocale } from '$lib/client/api-options'
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
import { createPagination } from '$lib/utils/value-object'
import { typedEntries } from '$lib/utils/typed-entries'
import { getSuccessfulResultMap } from '$lib/utils/result'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ url, locals, parent }) => {
	const auth = locals.accessToken
	const { platformAllowedActions } = await parent()

	const currentPage: number = getPageFromUrl(url)
	const perPage = 10

	const canCreateForum = platformAllowedActions.canManageStructure

	const forumsCount = (await getForumsCount<true>(withApiLocale({ auth, throwOnError: true }))).data

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
			await getForumsPaged<true>(
				withApiLocale({
					query: {
						...pagination
					},
					auth,
					throwOnError: true
				})
			)
		).data

		const forumIds = forums.map((forum) => forum.forumId)

		let forumCategoriesCount: Map<ForumId, Count>
		{
			const response = await getForumsCategoriesCount<true>(
				withApiLocale({
					path: { forumIds },
					auth,
					throwOnError: true
				})
			)
			forumCategoriesCount = getSuccessfulResultMap(response.data)
		}

		const forumsCategoriesLatest = new Map<ForumId, CategoryDto[]>()
		let categoryIds
		{
			const response = await getCategoriesPaged<true>(
				withApiLocale({
					query: { forumIds },
					auth,
					throwOnError: true
				})
			)
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
			const response = await getCategoriesThreadsCount<true>(
				withApiLocale({
					path: { categoryIds },
					auth,
					throwOnError: true
				})
			)
			categoriesThreadsCount = getSuccessfulResultMap(response.data)
		} else {
			categoriesThreadsCount = new Map()
		}

		let categoriesPostsCount: Map<CategoryId, Count>
		if (categoryIds.length > 0) {
			const response = await getCategoriesPostsCount<true>(
				withApiLocale({
					path: { categoryIds },
					auth,
					throwOnError: true
				})
			)
			categoriesPostsCount = getSuccessfulResultMap(response.data)
		} else {
			categoriesPostsCount = new Map()
		}

		let categoriesPostsLatest: Map<CategoryId, PostDto>
		if (categoryIds.length > 0) {
			const response = await getCategoriesPostsLatest<true>(
				withApiLocale({
					path: { categoryIds },
					auth,
					throwOnError: true
				})
			)
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
			const response = await getUsersBulk<true>(
				withApiLocale({
					path: { userIds: [...userIds] },
					auth,
					throwOnError: true
				})
			)
			users = getSuccessfulResultMap(response.data)
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
