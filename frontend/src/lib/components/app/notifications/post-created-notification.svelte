<script lang="ts">
	import NotificationEvent from '../notification-event.svelte'
	import type { NotifiableEventPayloadPostAddedNotifiableEventPayload } from '$lib/utils/client'
	import type { NotificationReferences } from './types'

	let {
		payload,
		occurredAt,
		users,
		threads,
		onNavigate
	}: NotificationReferences & {
		payload: NotifiableEventPayloadPostAddedNotifiableEventPayload
		occurredAt: Date
		onNavigate?: () => void
	} = $props()

	const authorUsername = $derived(users[payload.createdBy])
	const threadTitle = $derived(threads[payload.threadId])
</script>

<NotificationEvent
	actorId={payload.createdBy}
	actorUsername={authorUsername}
	eventType="post-added"
	threadId={payload.threadId}
	{threadTitle}
	{occurredAt}
	{onNavigate}
/>
