import {
	getCategory,
	getForum,
	getThreadPostsPaged,
	getThread,
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
import { redirect } from '@sveltejs/kit'
import { resolve } from '$app/paths'

const perPage = 10

export const load: PageServerLoad = async ({ params, url, locals }) => {
	const auth = locals.accessToken

	const threadId: ThreadId = params.threadId

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
		).data[`${threadId}`].value ?? 0

	const currentPage = getPageFromUrl(url)

	let threadData: { threadPosts: PostDto[]; users: Map<UserId, UserDto> } | undefined

	if (postCount !== 0) {
		const threadPosts = (
			await getThreadPostsPaged<true>({
				path: { threadId },
				query: {
					offset: (currentPage - 1) * perPage,
					limit: perPage
				},
				auth
			})
		).data

		const userIds = new Set(threadPosts.map((post) => post.createdBy))

		let users
		if (userIds.size > 0) {
			const response = await getUsersBulk<true>({ path: { userIds: [...userIds] } })
			users = new Map(
				Object.entries(response.data)
					.filter(([, item]) => item.value != null)
					.map(([key, item]) => [key, item.value!])
			)
		} else {
			users = new Map()
		}
		threadData = { threadPosts, users }
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
	const newPageIndex = postIndex / perPage + 1

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

		const threadId: ThreadId = params.threadId
		const auth = locals.accessToken

		if (!form.data.postId) {
			const postId = (
				await createPost<true>({
					path: { threadId },
					body: { content: form.data.content },
					auth
				})
			).data
			await navigateToPost(threadId, postId, auth)
		} else {
			const postId = form.data.postId
			await updatePost<true>({
				path: { postId },
				body: { content: form.data.content, rowVersion: form.data.rowVersion },
				auth
			})
			await navigateToPost(threadId, postId, auth)
		}
	}
}
