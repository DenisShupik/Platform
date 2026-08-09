<script lang="ts">
	import { resolve } from '$app/paths'
	import { PUBLIC_AVATAR_URL } from '$env/static/public'
	import * as Avatar from '$lib/components/ui/avatar'
	import type { ThreadId, UserId } from '$lib/utils/client'
	import { formatTimestamp } from '$lib/utils/formatTimestamp'
	import ClockIcon from '@lucide/svelte/icons/clock'

	let {
		actorId,
		actorUsername,
		action,
		threadId,
		threadTitle,
		occurredAt,
		onNavigate
	}: {
		actorId: UserId
		actorUsername?: string
		action: string
		threadId: ThreadId
		threadTitle?: string
		occurredAt: Date
		onNavigate?: () => void
	} = $props()

	let actorInitial = $derived(actorUsername?.at(0)?.toUpperCase() ?? '?')
</script>

<div class="flex min-w-0 flex-1 flex-row gap-4">
	<Avatar.Root class="size-8 place-self-center">
		<Avatar.Image
			src={`${PUBLIC_AVATAR_URL}/${actorId}`}
			alt={actorUsername ? `@${actorUsername}` : 'User avatar'}
		/>
		<Avatar.Fallback>{actorInitial}</Avatar.Fallback>
	</Avatar.Root>
	<div class="flex min-w-0 flex-1 flex-col justify-center gap-1">
		<p class="min-w-0">
			<span>{actorUsername ?? 'Unknown user'} {action} </span>
			<a
				class="text-primary hover:underline"
				onclick={onNavigate}
				href={resolve('/(app)/threads/[threadId=ThreadId]', { threadId })}
				>{threadTitle ?? 'Unknown thread'}</a
			>
		</p>
		<p class="flex items-center gap-x-1 text-xs text-muted-foreground">
			<ClockIcon class="size-3.5" aria-hidden="true" />
			<time datetime={occurredAt.toISOString()}>{formatTimestamp(occurredAt)}</time>
		</p>
	</div>
</div>
