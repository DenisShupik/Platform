import { withApiLocale } from '$lib/client/api-options'
import {
	getCategory,
	getForum,
	getThreadPostsPaged,
	getThread,
	getBookmarkedPostIds,
	getThreadsPostsCount,
	getUsersBulk,
	type ThreadId,
	type UserDto,
	type UserId,
	getThreadSubscriptionStatus,
	createPost,
	type PostId,
	getPostIndex,
	updatePost
} from '$lib/utils/client'
import { getPageFromUrl } from '$lib/utils/getPageFromUrl'
import { fail, setError, superValidate } from 'sveltekit-superforms'
import { valibot } from 'sveltekit-superforms/adapters'
import type { PageServerLoad } from './$types'
import { createPostSchema } from './utils'
import { error, redirect } from '@sveltejs/kit'
import { createPagination, parsePostContent, parsePostId, zeroCount } from '$lib/utils/value-object'
import { typedEntries } from '$lib/utils/typed-entries'
import { renderPosts, type RenderedPost } from '$lib/server/render-posts'
import { getLocale } from '$lib/paraglide/runtime'
import { resolve } from '$app/paths'
import * as m from '$lib/paraglide/messages'

const perPage = 10

export const load: PageServerLoad = async ({ params, url, locals }) => {
	const auth = locals.accessToken
	const userId = locals.userId

	const threadId = params.threadId

	const thread = (
		await getThread<true>(
			withApiLocale({
				path: { threadId },
				auth,
				throwOnError: true
			})
		)
	).data

	const category = (
		await getCategory<true>(
			withApiLocale({
				path: { categoryId: thread.categoryId },
				auth,
				throwOnError: true
			})
		)
	).data

	const forum = (
		await getForum<true>(
			withApiLocale({
				path: { forumId: category.forumId },
				auth,
				throwOnError: true
			})
		)
	).data

	const postCount =
		(
			await getThreadsPostsCount<true>(
				withApiLocale({
					path: { threadIds: [threadId] },
					auth,
					throwOnError: true
				})
			)
		).data[threadId]?.value ?? zeroCount

	let currentPage = getPageFromUrl(url)
	const postId = parsePostId(url.searchParams.get('post'))
	if (postId !== undefined) {
		const postIndex = await getPostIndex<true>(
			withApiLocale({
				path: { postId },
				auth,
				throwOnError: true
			})
		)
		currentPage = Math.floor(postIndex.data / perPage) + 1
	}

	let threadData:
		| {
				threadPosts: RenderedPost[]
				users: Map<UserId, UserDto>
				bookmarkedPostIds: PostId[]
		  }
		| undefined

	if (postCount !== 0) {
		const pagination = createPagination(currentPage, perPage)
		const posts = (
			await getThreadPostsPaged<true>(
				withApiLocale({
					path: { threadId },
					query: {
						...pagination
					},
					auth,
					throwOnError: true
				})
			)
		).data
		const threadPosts = renderPosts(posts)

		const userIds = new Set(threadPosts.map((post) => post.createdBy))

		let users: Map<UserId, UserDto>
		if (userIds.size > 0) {
			const response = await getUsersBulk<true>(
				withApiLocale({ path: { userIds: [...userIds] }, throwOnError: true })
			)
			users = new Map(
				typedEntries(response.data).flatMap(([userId, item]) =>
					item?.value == null ? [] : [[userId, item.value] as const]
				)
			)
		} else {
			users = new Map()
		}

		const bookmarkedPostIds = auth
			? (
					await getBookmarkedPostIds<true>(
						withApiLocale({
							path: { postIds: threadPosts.map((post) => post.postId) },
							auth,
							throwOnError: true
						})
					)
				).data.postIds
			: []

		threadData = { threadPosts, users, bookmarkedPostIds }
	}

	const isSubscribed =
		auth && userId
			? (
					await getThreadSubscriptionStatus<true>(
						withApiLocale({
							path: { userId, threadId },
							auth,
							throwOnError: true
						})
					)
				).data.isSubscribed
			: false

	const form = await superValidate(valibot(createPostSchema(), { config: { lang: getLocale() } }))

	return {
		thread,
		category,
		forum,
		currentPage,
		perPage,
		postCount,
		threadData,
		isSubscribed,
		form
	}
}

async function navigateToPost(threadId: ThreadId, postId: PostId, auth: string) {
	const postIndex = (
		await getPostIndex<true>(withApiLocale({ path: { postId }, auth, throwOnError: true }))
	).data
	const newPageIndex = Math.floor(postIndex / perPage) + 1

	throw redirect(
		303,
		`${resolve('/(app)/threads/[threadId=ThreadId]', { threadId })}?page=${newPageIndex}#post-${postId}`
	)
}

export const actions = {
	default: async ({ params, request, locals }) => {
		const form = await superValidate(
			request,
			valibot(createPostSchema(), { config: { lang: getLocale() } })
		)

		if (!form.valid) {
			return fail(400, { form })
		}

		const threadId = params.threadId
		const auth = locals.accessToken
		if (!auth) error(401, m.error_unauthorized())
		const content = parsePostContent(form.data.content)
		if (content === undefined) return fail(400, { form })

		if (!form.data.postId) {
			const result = await createPost<false>(
				withApiLocale({
					path: { threadId },
					body: { content },
					auth,
					throwOnError: false
				})
			)
			if (result.error) {
				if (result.response?.status === 400) {
					return setError(form, 'content', m.validation_disallowed_post())
				}
				throw error(result.response?.status ?? 500, m.post_create_failed())
			}
			if (result.data === undefined) error(500, m.post_create_failed())

			const postId = result.data
			await navigateToPost(threadId, postId, auth)
		} else {
			const postId = parsePostId(form.data.postId)
			if (postId === undefined) return fail(400, { form })
			if (form.data.rowVersion === undefined) return fail(400, { form })
			const result = await updatePost<false>(
				withApiLocale({
					path: { postId },
					body: {
						content,
						rowVersion: form.data.rowVersion
					},
					auth,
					throwOnError: false
				})
			)
			if (result.error) {
				if (result.response?.status === 400) {
					return setError(form, 'content', m.validation_disallowed_post())
				}
				throw error(result.response?.status ?? 500, m.post_update_failed())
			}

			await navigateToPost(threadId, postId, auth)
		}
	}
}
