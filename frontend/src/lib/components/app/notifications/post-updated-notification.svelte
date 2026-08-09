<script lang="ts">
	import NotificationEvent from '../notification-event.svelte'
	import type { NotifiableEventPayloadPostUpdatedNotifiableEventPayload } from '$lib/utils/client'
	import type { NotificationReferences } from './types'

	let {
		payload,
		occurredAt,
		users,
		threads,
		onNavigate
	}: NotificationReferences & {
		payload: NotifiableEventPayloadPostUpdatedNotifiableEventPayload
		occurredAt: Date
		onNavigate?: () => void
	} = $props()

	const authorUsername = $derived(users[payload.updatedBy])
	const threadTitle = $derived(threads[payload.threadId])
</script>

<NotificationEvent
	actorId={payload.updatedBy}
	actorUsername={authorUsername}
	action="updated a post in"
	threadId={payload.threadId}
	{threadTitle}
	{occurredAt}
	{onNavigate}
/>
