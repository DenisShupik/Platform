<script lang="ts">
	import NotificationEvent from '../notification-event.svelte'
	import type { NotifiableEventPayloadThreadRejectedNotifiableEventPayload } from '$lib/utils/client'
	import type { NotificationReferences } from './types'

	let {
		payload,
		occurredAt,
		users,
		threads,
		onNavigate
	}: NotificationReferences & {
		payload: NotifiableEventPayloadThreadRejectedNotifiableEventPayload
		occurredAt: Date
		onNavigate?: () => void
	} = $props()

	const rejectedByUsername = $derived(users[payload.rejectedBy])
	const threadTitle = $derived(threads[payload.threadId])
</script>

<NotificationEvent
	actorId={payload.rejectedBy}
	actorUsername={rejectedByUsername}
	action="rejected"
	threadId={payload.threadId}
	{threadTitle}
	{occurredAt}
	{onNavigate}
/>
