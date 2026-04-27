<script lang="ts">
	import IconEyeCheck from '~icons/tabler/eye-check'
	import IconTrash from '~icons/tabler/trash'
	import { Button } from '$lib/components/ui/button'
	import {
		deleteInternalNotification,
		markInternalNotificationAsRead,
		type InternalNotificationDto
	} from '$lib/utils/client'
	import { internalNotificationStore } from '$lib/client/internal-notification-store.svelte'
	import {
		PostCreatedNotification,
		PostUpdatedNotification,
		ThreadApprovedNotification,
		ThreadRejectedNotification
	} from '$lib/components/app'
	import { Spinner } from '$lib/components/ui/spinner'

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
				path: { notifiableEventId: notification.notifiableEventId }
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
				path: { notifiableEventId: notification.notifiableEventId }
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
		isProcessing ? 'cursor-not-allowed' : 'cursor-pointer hover:bg-muted/50'
	}`}
>
	{#if isProcessing}
		<div
			class="absolute inset-0 z-10 flex w-full items-center justify-center bg-background/50 backdrop-blur-[2px]"
		>
			<Spinner class="size-6" />
		</div>
	{/if}

	{#if notification.payload.$type === 'PostAdded'}
		<PostCreatedNotification payload={notification.payload} occurredAt={notification.occurredAt} />
	{:else if notification.payload.$type === 'PostUpdated'}
		<PostUpdatedNotification payload={notification.payload} occurredAt={notification.occurredAt} />
	{:else if notification.payload.$type === 'ThreadApproved'}
		<ThreadApprovedNotification
			payload={notification.payload}
			occurredAt={notification.occurredAt}
		/>
	{:else if notification.payload.$type === 'ThreadRejected'}
		<ThreadRejectedNotification
			payload={notification.payload}
			occurredAt={notification.occurredAt}
		/>
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
