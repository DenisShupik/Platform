<script lang="ts">
	import { resolve } from '$app/paths'
	import * as Avatar from '$lib/components/ui/avatar'
	import { PUBLIC_AVATAR_URL } from '$env/static/public'
	import { formatTimestamp } from '$lib/utils/formatTimestamp'
	import IconClockFilled from '~icons/tabler/clock-filled'
	import type { NotifiableEventPayloadThreadApprovedNotifiableEventPayload } from '$lib/utils/client'
	import { internalNotificationStore } from '$lib/client/internal-notification-store.svelte'

	const {
		payload,
		occurredAt
	}: {
		payload: NotifiableEventPayloadThreadApprovedNotifiableEventPayload
		occurredAt: Date
	} = $props()

	const approvedByUsername = $derived($internalNotificationStore.users[payload.approvedBy])
	const threadTitle = $derived($internalNotificationStore.threads[payload.threadId])
</script>

<div class="flex flex-1 flex-row space-x-4">
	<Avatar.Root class="size-8 place-self-center">
		<Avatar.Image src="{PUBLIC_AVATAR_URL}/{payload.approvedBy}" alt="@{approvedByUsername}" />
		<Avatar.Fallback>{approvedByUsername}</Avatar.Fallback>
	</Avatar.Root>
	<div class="flex flex-1 flex-col justify-center space-y-1">
		<p>
			<span>{approvedByUsername ?? '—'}</span>
			<span>approved</span>
			<a
				class="text-blue-600 hover:underline"
				href={resolve('/(app)/threads/[threadId=ThreadId]', {
					threadId: payload.threadId
				})}>{threadTitle ?? '—'}</a
			>
		</p>
		<p class="text-muted-foreground flex items-center gap-x-1 text-xs">
			<IconClockFilled class="inline size-3" />
			<time>{formatTimestamp(occurredAt)}</time>
		</p>
	</div>
</div>
