<script lang="ts">
	import { resolve } from '$app/paths'
	import * as Avatar from '$lib/components/ui/avatar'
	import { PUBLIC_AVATAR_URL } from '$env/static/public'
	import { formatTimestamp } from '$lib/utils/formatTimestamp'
	import { IconClockFilled, IconEyeCheck, IconLoader2, IconTrash } from '@tabler/icons-svelte'
	import { Button } from '$lib/components/ui/button'
	import {
		deleteInternalNotification,
		markInternalNotificationAsRead,
		type InternalNotificationDto
	} from '$lib/utils/client'
	import { currentUser } from '$lib/client/current-user-state.svelte'
	import { internalNotificationStore } from '$lib/client/internal-notification-store.svelte'

	const {
		notification
	}: {
		notification: InternalNotificationDto
	} = $props()

	let isProcessing = $state(false)

	const authorUsername = $derived($internalNotificationStore.users[notification.payload.createdBy])
	const threadTitle = $derived($internalNotificationStore.threads[notification.payload.threadId])

	async function handleMarkRead() {
		if (isProcessing) return

		try {
			isProcessing = true
			await markInternalNotificationAsRead({
				path: {notifiableEventId: notification.notifiableEventId },
				auth: currentUser.user?.token
			})
			internalNotificationStore.update()
		} catch (error) {
			console.error('Failed to delete notification:', error)
		} finally {
			isProcessing = false
		}
	}

	async function handleDelete() {
		if (isProcessing) return

		try {
			isProcessing = true
			await deleteInternalNotification<true>({
				path: { notifiableEventId: notification.notifiableEventId },
				auth: currentUser.user?.token
			})
			internalNotificationStore.update()
		} catch (error) {
			console.error('Failed to delete notification:', error)
		} finally {
			isProcessing = false
		}
	}
</script>

<li
	class={`relative flex flex-row space-x-4 p-3 font-medium ${
		isProcessing ? 'cursor-not-allowed' : 'hover:bg-muted/50 cursor-pointer'
	}`}
>
	{#if isProcessing}
		<div
			class="bg-background/50 absolute inset-0 z-10 flex w-full items-center justify-center backdrop-blur-[2px]"
		>
			<IconLoader2 class="size-6 animate-spin" />
		</div>
	{/if}
	<Avatar.Root class="size-8 place-self-center">
		<Avatar.Image src="{PUBLIC_AVATAR_URL}/{notification.payload.createdBy}" alt="@shadcn" />
		<Avatar.Fallback>{authorUsername}</Avatar.Fallback>
	</Avatar.Root>
	<div class="flex flex-1 flex-col justify-center space-y-1">
		<p>
			<span>{authorUsername ?? '—'}</span>
			<span>posted to</span>
			<a
				class="text-blue-600 hover:underline"
				href={resolve('/(app)/threads/[threadId=ThreadId]', {
					threadId: notification.payload.threadId
				})}>{threadTitle ?? '—'}</a
			>
		</p>
		<p class="text-muted-foreground flex items-center gap-x-1 text-xs">
			<IconClockFilled class="inline size-3" />
			<time>{formatTimestamp(notification.occurredAt)}</time>
		</p>
	</div>
	<div class="flex flex-col space-y-2 place-self-center">
		<Button
			variant="outline"
			size="icon"
			class="size-6 cursor-pointer"
			disabled={isProcessing}
			onclick={handleMarkRead}
		>
			<IconEyeCheck />
		</Button>
		<Button
			variant="destructive"
			size="icon"
			class="size-6 cursor-pointer"
			disabled={isProcessing}
			onclick={handleDelete}
		>
			<IconTrash />
		</Button>
	</div>
</li>
