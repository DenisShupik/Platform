import { authStore, currentUser } from '$lib/client/auth-state.svelte'
import { getThreadSubscriptionStatus, type ThreadId } from '$lib/utils/client'
import { get } from 'svelte/store'
import type { PageLoad } from './$types'

export const ssr = false
export const csr = true

export const load: PageLoad = async ({ parent, data, params, fetch }) => {
	await parent()

	const threadId: ThreadId = params.threadId

	const userId = get(currentUser)?.id

	if (!userId) return { ...data }

	const { data: isSubscribed } = await getThreadSubscriptionStatus<true>({
		path: { threadId },
		fetch,
		auth: get(authStore).token
	})

	return { ...data, ...isSubscribed }
}
