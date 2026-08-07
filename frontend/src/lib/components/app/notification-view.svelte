<script lang="ts">
	import MailCheckIcon from '@lucide/svelte/icons/mail-check'
	import Trash2Icon from '@lucide/svelte/icons/trash-2'
	import { Button } from '$lib/components/ui/button'
	import {
		deleteInternalNotification,
		markInternalNotificationAsRead,
		type InternalNotificationDto
	} from '$lib/utils/client'
	import {
		PostCreatedNotification,
		PostUpdatedNotification,
		ThreadApprovedNotification,
		ThreadRejectedNotification
	} from '$lib/components/app'
	import { Spinner } from '$lib/components/ui/spinner'
	import { cn } from '$lib/utils.js'
	import type { NotificationReferences } from './notifications/types'

	let {
		notification,
		users,
		threads,
		onChange,
		onNavigate
	}: NotificationReferences & {
		notification: InternalNotificationDto
		onChange?: () => void | Promise<void>
		onNavigate?: () => void
	} = $props()

	let isProcessing = $state(false)

	async function handleMarkRead() {
		if (isProcessing) return

		try {
			isProcessing = true
			await markInternalNotificationAsRead({
				path: { notifiableEventId: notification.notifiableEventId }
			})
			await onChange?.()
		} catch (error) {
			console.error('Failed to mark notification as read:', error)
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
			await onChange?.()
		} catch (error) {
			console.error('Failed to delete notification:', error)
		} finally {
			isProcessing = false
		}
	}
</script>

<li
	class={cn(
		'relative flex flex-row gap-4 p-3 font-medium hover:bg-muted/50',
		isProcessing && 'cursor-not-allowed'
	)}
>
	{#if isProcessing}
		<div
			class="absolute inset-0 z-10 flex w-full items-center justify-center bg-background/50 backdrop-blur-[2px]"
		>
			<Spinner class="size-6" />
		</div>
	{/if}

	{#if notification.payload.$type === 'PostAdded'}
		<PostCreatedNotification
			payload={notification.payload}
			occurredAt={notification.occurredAt}
			{users}
			{threads}
			{onNavigate}
		/>
	{:else if notification.payload.$type === 'PostUpdated'}
		<PostUpdatedNotification
			payload={notification.payload}
			occurredAt={notification.occurredAt}
			{users}
			{threads}
			{onNavigate}
		/>
	{:else if notification.payload.$type === 'ThreadApproved'}
		<ThreadApprovedNotification
			payload={notification.payload}
			occurredAt={notification.occurredAt}
			{users}
			{threads}
			{onNavigate}
		/>
	{:else if notification.payload.$type === 'ThreadRejected'}
		<ThreadRejectedNotification
			payload={notification.payload}
			occurredAt={notification.occurredAt}
			{users}
			{threads}
			{onNavigate}
		/>
	{/if}

	<div class="flex flex-col gap-2 place-self-center">
		{#if notification.deliveredAt == null}
			<Button
				variant="outline"
				size="icon-xs"
				aria-label="Mark as read"
				disabled={isProcessing}
				onclick={handleMarkRead}
			>
				<MailCheckIcon data-icon />
			</Button>
		{/if}
		<Button
			variant="destructive"
			size="icon-xs"
			aria-label="Delete notification"
			disabled={isProcessing}
			onclick={handleDelete}
		>
			<Trash2Icon data-icon />
		</Button>
	</div>
</li>
