import { currentUser } from '$lib/client/current-user-state.svelte'
import { getThreadSubscriptionStatus, type ThreadId } from '$lib/utils/client'
import type { PageLoad } from './$types'

export const ssr = false
export const csr = true

export const load: PageLoad = async ({ parent, data, params, fetch }) => {
	await parent()

	const threadId: ThreadId = params.threadId

	const userId = currentUser.user?.id

	if (!userId) return { ...data }

	const { data: isSubscribed } = await getThreadSubscriptionStatus<true>({
		path: { threadId },
		fetch,
		auth: currentUser.user?.token
	})

	return { ...data, ...isSubscribed }
}
