import {
	getBookmarkedPostsCount,
	getBookmarkedPostsPaged,
	getThreadsBulk,
	getUsersBulk,
	type ThreadDto,
	type ThreadId,
	type UserDto,
	type UserId
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import { typedEntries } from '$lib/utils/typed-entries'
import { createPagination } from '$lib/utils/value-object'
import { renderPosts, type RenderedPost } from '$lib/server/render-posts'
import { error } from '@sveltejs/kit'
import type { PageServerLoad } from './$types'

const perPage = 10

export const load: PageServerLoad = async ({ url, locals }) => {
	const auth = locals.accessToken
	const userId = locals.userId
	if (!auth || !userId) error(401, 'Unauthorized')

	const bookmarkedPostsCount = (await getBookmarkedPostsCount<true>({ path: { userId }, auth }))
		.data
	const currentPage = getPageFromUrl(url)

	let bookmarksData:
		| {
				bookmarkedPosts: RenderedPost[]
				threads: Map<ThreadId, ThreadDto>
				users: Map<UserId, UserDto>
		  }
		| undefined

	if (bookmarkedPostsCount !== 0) {
		const posts = (
			await getBookmarkedPostsPaged<true>({
				path: { userId },
				query: createPagination(currentPage, perPage),
				auth
			})
		).data
		const bookmarkedPosts = renderPosts(posts)

		const threadIds = new Set(bookmarkedPosts.map((post) => post.threadId))
		const userIds = new Set(bookmarkedPosts.map((post) => post.createdBy))

		const [threadsResponse, usersResponse] = await Promise.all([
			getThreadsBulk<true>({ path: { threadIds: [...threadIds] }, auth }),
			getUsersBulk<true>({ path: { userIds: [...userIds] }, auth })
		])

		const threads = new Map(
			typedEntries(threadsResponse.data).flatMap(([threadId, item]) =>
				item?.value == null ? [] : [[threadId, item.value] as const]
			)
		)
		const users = new Map(
			typedEntries(usersResponse.data).flatMap(([userId, item]) =>
				item?.value == null ? [] : [[userId, item.value] as const]
			)
		)

		bookmarksData = { bookmarkedPosts, threads, users }
	}

	return { bookmarkedPostsCount, currentPage, perPage, bookmarksData }
}
