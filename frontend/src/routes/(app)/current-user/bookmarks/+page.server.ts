import { withApiLocale } from '$lib/client/api-options'
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
import { getSuccessfulResultMap } from '$lib/utils/result'
import { createPagination } from '$lib/utils/value-object'
import { renderPosts, type RenderedPost } from '$lib/server/render-posts'
import { error } from '@sveltejs/kit'
import * as m from '$lib/paraglide/messages'
import type { PageServerLoad } from './$types'

const perPage = 10

export const load: PageServerLoad = async ({ url, locals }) => {
	const auth = locals.accessToken
	const userId = locals.userId
	if (!auth || !userId) error(401, m.error_unauthorized())

	const bookmarkedPostsCount = (
		await getBookmarkedPostsCount<true>(
			withApiLocale({ path: { userId }, auth, throwOnError: true })
		)
	).data
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
			await getBookmarkedPostsPaged<true>(
				withApiLocale({
					path: { userId },
					query: createPagination(currentPage, perPage),
					auth,
					throwOnError: true
				})
			)
		).data
		const bookmarkedPosts = renderPosts(posts)

		const threadIds = new Set(bookmarkedPosts.map((post) => post.threadId))
		const userIds = new Set(bookmarkedPosts.map((post) => post.createdBy))

		const [threadsResponse, usersResponse] = await Promise.all([
			getThreadsBulk<true>(
				withApiLocale({ path: { threadIds: [...threadIds] }, auth, throwOnError: true })
			),
			getUsersBulk<true>(
				withApiLocale({ path: { userIds: [...userIds] }, auth, throwOnError: true })
			)
		])

		const threads = getSuccessfulResultMap(threadsResponse.data)
		const users = getSuccessfulResultMap(usersResponse.data)

		bookmarksData = { bookmarkedPosts, threads, users }
	}

	return { bookmarkedPostsCount, currentPage, perPage, bookmarksData }
}
