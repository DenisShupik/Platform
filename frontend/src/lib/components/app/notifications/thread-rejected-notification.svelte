<script lang="ts">
	import { resolve } from '$app/paths'
	import * as Avatar from '$lib/components/ui/avatar'
	import { PUBLIC_AVATAR_URL } from '$env/static/public'
	import { formatTimestamp } from '$lib/utils/formatTimestamp'
	import ClockIcon from '@lucide/svelte/icons/clock'
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
	const rejectedByInitial = $derived(rejectedByUsername?.at(0)?.toUpperCase() ?? '?')
	const threadTitle = $derived(threads[payload.threadId])
</script>

<div class="flex flex-1 flex-row gap-4">
	<Avatar.Root class="size-8 place-self-center">
		<Avatar.Image src="{PUBLIC_AVATAR_URL}/{payload.rejectedBy}" alt="@{rejectedByUsername}" />
		<Avatar.Fallback>{rejectedByInitial}</Avatar.Fallback>
	</Avatar.Root>
	<div class="flex min-w-0 flex-1 flex-col justify-center gap-1">
		<p class="min-w-0">
			<span>{rejectedByUsername ?? '—'}</span>
			<span>rejected</span>
			<a
				class="text-primary hover:underline"
				onclick={onNavigate}
				href={resolve('/(app)/threads/[threadId=ThreadId]', {
					threadId: payload.threadId
				})}>{threadTitle ?? '—'}</a
			>
		</p>
		<p class="flex items-center gap-x-1 text-xs text-muted-foreground">
			<ClockIcon class="size-3" />
			<time>{formatTimestamp(occurredAt)}</time>
		</p>
	</div>
</div>
