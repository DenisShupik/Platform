<script lang="ts">
	import { IconEyeCheck, IconLoader2, IconTrash } from '@tabler/icons-svelte'
	import { Button } from '$lib/components/ui/button'
	import {
		deleteInternalNotification,
		markInternalNotificationAsRead,
		type NotifiableEventPayload,
		type InternalNotificationDto
	} from '$lib/utils/client'
	import { currentUser } from '$lib/client/current-user-state.svelte'
	import { internalNotificationStore } from '$lib/client/internal-notification-store.svelte'
	import { PostCreatedNotification, PostUpdatedNotification } from '$lib/components/app'

	const {
		notification
	}: {
		notification: InternalNotificationDto
	} = $props()

	let isProcessing = $state(false)

	async function handleMarkRead() {
		if (isProcessing) return

		try {
			isProcessing = true
			await markInternalNotificationAsRead({
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

	{#if notification.payload.$type === 'PostAdded'}
		<PostCreatedNotification payload={notification.payload} occurredAt={notification.occurredAt} />
	{:else if notification.payload.$type === 'PostUpdated'}
		<PostUpdatedNotification payload={notification.payload} occurredAt={notification.occurredAt} />
	{/if}

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
