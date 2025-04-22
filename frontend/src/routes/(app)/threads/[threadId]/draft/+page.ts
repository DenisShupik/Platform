import { authStore, currentUser } from '$lib/client/auth-state.svelte'
import { getCategory, getForum, getThread, type ThreadId } from '$lib/utils/client'
import { zThreadId } from '$lib/utils/client/zod.gen'
import { get } from 'svelte/store'
import type { PageLoad } from '../../drafts/[threadId]/$types'
import { error, redirect } from '@sveltejs/kit'

export const ssr = false
export const csr = true

export const load: PageLoad = async ({ params }) => {
	const parseResult = zThreadId.safeParse(params.threadId)

	if (!parseResult.success) {
		error(400, {
			message: 'Invalid thread ID'
		})
	}

	const threadId: ThreadId = parseResult.data

	const userId = get(currentUser)?.id
	if (!userId) {
		await get(authStore).login()
	}

	const thread = (await getThread<true>({ path: { threadId }, auth: get(authStore).token })).data

	if (thread.status !== 0) redirect(308, '/threads/' + threadId)

	const category = (await getCategory<true>({ path: { categoryId: thread.categoryId } })).data

	const forum = (await getForum<true>({ path: { forumId: category.forumId } })).data

	return {
		thread,
		category,
		forum
	}
}
