import {
	getCategory,
	getForum,
	getThreadPostsPaged,
	getThread,
	getBookmarkedPostIds,
	getThreadsPostsCount,
	getUsersBulk,
	type PostDto,
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
import { fail, superValidate } from 'sveltekit-superforms'
import { valibot } from 'sveltekit-superforms/adapters'
import type { PageServerLoad } from './$types'
import { postSchema } from './utils'
import { error, redirect } from '@sveltejs/kit'
import { resolve } from '$app/paths'
import { createPagination, parsePostContent, parsePostId, zeroCount } from '$lib/utils/value-object'
import { typedEntries } from '$lib/utils/typed-entries'

const perPage = 10

export const load: PageServerLoad = async ({ params, url, locals }) => {
	const auth = locals.accessToken

	const threadId = params.threadId

	const thread = (
		await getThread<true>({
			path: { threadId },
			auth
		})
	).data

	const category = (
		await getCategory<true>({
			path: { categoryId: thread.categoryId },
			auth
		})
	).data

	const forum = (
		await getForum<true>({
			path: { forumId: category.forumId },
			auth
		})
	).data

	const postCount =
		(
			await getThreadsPostsCount<true>({
				path: { threadIds: [threadId] },
				auth
			})
		).data[threadId]?.value ?? zeroCount

	let currentPage = getPageFromUrl(url)
	const postId = parsePostId(url.searchParams.get('post'))
	if (postId !== undefined) {
		const postIndex = await getPostIndex<true>({
			path: { postId },
			auth
		})
		currentPage = Math.floor(postIndex.data / perPage) + 1
	}

	let threadData:
		| {
				threadPosts: PostDto[]
				users: Map<UserId, UserDto>
				bookmarkedPostIds: PostId[]
		  }
		| undefined

	if (postCount !== 0) {
		const pagination = createPagination(currentPage, perPage)
		const threadPosts = (
			await getThreadPostsPaged<true>({
				path: { threadId },
				query: {
					...pagination
				},
				auth
			})
		).data

		const userIds = new Set(threadPosts.map((post) => post.createdBy))

		let users: Map<UserId, UserDto>
		if (userIds.size > 0) {
			const response = await getUsersBulk<true>({ path: { userIds: [...userIds] } })
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
					await getBookmarkedPostIds<true>({
						path: { postIds: threadPosts.map((post) => post.postId) },
						auth
					})
				).data.postIds
			: []

		threadData = { threadPosts, users, bookmarkedPostIds }
	}

	const isSubscribed = auth
		? (
				await getThreadSubscriptionStatus<true>({
					path: { threadId },
					auth
				})
			).data.isSubscribed
		: false

	const form = await superValidate(valibot(postSchema))

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
	const postIndex = (await getPostIndex<true>({ path: { postId }, auth })).data
	const newPageIndex = Math.floor(postIndex / perPage) + 1

	throw redirect(
		303,
		`${resolve('/(app)/threads/[threadId=ThreadId]', { threadId })}?page=${newPageIndex}#post-${postId}`
	)
}

export const actions = {
	default: async ({ params, request, locals }) => {
		const form = await superValidate(request, valibot(postSchema))

		if (!form.valid) {
			return fail(400, { form })
		}

		const threadId = params.threadId
		const auth = locals.accessToken
		if (!auth) error(401, 'Unauthorized')
		const content = parsePostContent(form.data.content)
		if (content === undefined) return fail(400, { form })

		if (!form.data.postId) {
			const postId = (
				await createPost<true>({
					path: { threadId },
					body: { content },
					auth
				})
			).data
			await navigateToPost(threadId, postId, auth)
		} else {
			const postId = parsePostId(form.data.postId)
			if (postId === undefined) return fail(400, { form })
			if (form.data.rowVersion === undefined) return fail(400, { form })
			await updatePost<true>({
				path: { postId },
				body: {
					content,
					rowVersion: form.data.rowVersion
				},
				auth
			})
			await navigateToPost(threadId, postId, auth)
		}
	}
}
