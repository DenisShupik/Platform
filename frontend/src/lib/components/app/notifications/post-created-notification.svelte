<script lang="ts">
	import { resolve } from '$app/paths'
	import * as Avatar from '$lib/components/ui/avatar'
	import { PUBLIC_AVATAR_URL } from '$env/static/public'
	import { formatTimestamp } from '$lib/utils/formatTimestamp'
	import { IconClockFilled } from '@tabler/icons-svelte'
	import type { PostAddedNotifiableEventPayload } from '$lib/utils/client'
	import { internalNotificationStore } from '$lib/client/internal-notification-store.svelte'

	const {
		payload,
		occurredAt
	}: {
		payload: PostAddedNotifiableEventPayload
		occurredAt: Date
	} = $props()

	const authorUsername = $derived($internalNotificationStore.users[payload.createdBy])
	const threadTitle = $derived($internalNotificationStore.threads[payload.threadId])
</script>

<div class="flex flex-row space-x-4">
	<Avatar.Root class="size-8 place-self-center">
		<Avatar.Image src="{PUBLIC_AVATAR_URL}/{payload.createdBy}" alt="@{authorUsername}" />
		<Avatar.Fallback>{authorUsername}</Avatar.Fallback>
	</Avatar.Root>
	<div class="flex flex-1 flex-col justify-center space-y-1">
		<p>
			<span>{authorUsername ?? '—'}</span>
			<span>posted to</span>
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
