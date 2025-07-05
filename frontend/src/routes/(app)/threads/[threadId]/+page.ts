import { authStore, currentUser } from '$lib/client/auth-state.svelte'
import { getThreadSubscriptionStatus, type ThreadId } from '$lib/utils/client'
import { zThreadId } from '$lib/utils/client/zod.gen'
import { get } from 'svelte/store'
import type { PageLoad } from './$types'
import { error } from '@sveltejs/kit'

export const ssr = false
export const csr = true

export const load: PageLoad = async ({ parent, data, params, fetch }) => {
	await parent()
	const parseResult = zThreadId.safeParse(params.threadId)

	if (!parseResult.success) {
		error(400, {
			message: 'Invalid thread ID'
		})
	}

	const threadId: ThreadId = parseResult.data

	const userId = get(currentUser)?.id

	if (!userId) return { ...data }
	
	const { data: isSubscribed } = await getThreadSubscriptionStatus<true>({
		path: { threadId },
		fetch,
		auth: get(authStore).token
	})

	return { ...data, ...isSubscribed }
}
