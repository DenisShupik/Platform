<script lang="ts">
	import NotificationEvent from '../notification-event.svelte'
	import type { NotifiableEventPayloadThreadApprovedNotifiableEventPayload } from '$lib/utils/client'
	import type { NotificationReferences } from './types'

	let {
		payload,
		occurredAt,
		users,
		threads,
		onNavigate
	}: NotificationReferences & {
		payload: NotifiableEventPayloadThreadApprovedNotifiableEventPayload
		occurredAt: Date
		onNavigate?: () => void
	} = $props()

	const approvedByUsername = $derived(users[payload.approvedBy])
	const threadTitle = $derived(threads[payload.threadId])
</script>

<NotificationEvent
	actorId={payload.approvedBy}
	actorUsername={approvedByUsername}
	action="approved"
	threadId={payload.threadId}
	{threadTitle}
	{occurredAt}
	{onNavigate}
/>
