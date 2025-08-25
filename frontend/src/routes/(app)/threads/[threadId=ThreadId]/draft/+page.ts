import { currentUser, login } from '$lib/client/current-user-state.svelte'
import { getCategory, getForum, getThread, type ThreadId } from '$lib/utils/client'
import type { PageLoad } from './$types'
import { redirect } from '@sveltejs/kit'

export const ssr = false
export const csr = true

export const load: PageLoad = async ({ params, fetch }) => {
	const threadId: ThreadId = params.threadId

	const userId = currentUser.user?.id
	if (!userId) {
		await login()
	}

	const thread = (
		await getThread<true>({ path: { threadId }, fetch, auth: currentUser.user?.token })
	).data

	if (thread.status !== 0) redirect(308, '/threads/' + threadId)

	const category = (await getCategory<true>({ path: { categoryId: thread.categoryId }, fetch }))
		.data

	const forum = (await getForum<true>({ path: { forumId: category.forumId }, fetch })).data

	return {
		thread,
		category,
		forum
	}
}
