<script lang="ts">
	import { resolve } from '$app/paths'
	import { PUBLIC_AVATAR_URL } from '$env/static/public'
	import * as Avatar from '$lib/components/ui/avatar'
	import type { ThreadId, UserId } from '$lib/utils/client'
	import { formatRelativeTimestamp, formatTimestamp } from '$lib/utils/format'
	import ClockIcon from '@lucide/svelte/icons/clock'
	import * as m from '$lib/paraglide/messages'
	import { onMount } from 'svelte'

	type NotificationEventType = 'post-added' | 'post-updated' | 'thread-approved' | 'thread-rejected'

	let {
		actorId,
		actorUsername,
		eventType,
		threadId,
		threadTitle,
		occurredAt,
		onNavigate
	}: {
		actorId: UserId
		actorUsername?: string
		eventType: NotificationEventType
		threadId: ThreadId
		threadTitle?: string
		occurredAt: Date
		onNavigate?: () => void
	} = $props()

	let actorInitial = $derived(actorUsername?.at(0)?.toUpperCase() ?? '?')
	let relativeTimestamp = $state<string>()
	let notificationMessage = $derived.by(() => {
		const inputs = {
			actor: actorUsername ?? m.user_unknown(),
			thread: threadTitle ?? m.thread_unknown()
		}

		switch (eventType) {
			case 'post-added':
				return m.notification_posted(inputs)
			case 'post-updated':
				return m.notification_updated(inputs)
			case 'thread-approved':
				return m.notification_approved(inputs)
			case 'thread-rejected':
				return m.notification_rejected(inputs)
		}
	})

	onMount(() => {
		const updateRelativeTimestamp = () => (relativeTimestamp = formatRelativeTimestamp(occurredAt))
		updateRelativeTimestamp()
		const intervalId = setInterval(updateRelativeTimestamp, 60_000)
		return () => clearInterval(intervalId)
	})
</script>

<div class="flex min-w-0 flex-1 flex-row gap-4">
	<Avatar.Root class="size-8 place-self-center">
		<Avatar.Image
			src={`${PUBLIC_AVATAR_URL}/${actorId}`}
			alt={actorUsername ? `@${actorUsername}` : m.user_avatar()}
		/>
		<Avatar.Fallback>{actorInitial}</Avatar.Fallback>
	</Avatar.Root>
	<div class="flex min-w-0 flex-1 flex-col justify-center gap-1">
		<p class="min-w-0">
			<a
				class="text-foreground hover:text-primary hover:underline"
				onclick={onNavigate}
				href={resolve('/(app)/threads/[threadId=ThreadId]', { threadId })}>{notificationMessage}</a
			>
		</p>
		<p class="flex items-center gap-x-1 text-xs text-muted-foreground">
			<ClockIcon class="size-3.5" aria-hidden="true" />
			<time datetime={occurredAt.toISOString()} title={formatTimestamp(occurredAt)}
				>{relativeTimestamp ?? formatTimestamp(occurredAt)}</time
			>
		</p>
	</div>
</div>
