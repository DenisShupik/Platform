<script lang="ts">
	import BellIcon from '@lucide/svelte/icons/bell'
	import { resolve } from '$app/paths'
	import { buttonVariants, Button } from '$lib/components/ui/button'
	import { Badge } from '$lib/components/ui/badge'
	import * as Popover from '$lib/components/ui/popover'
	import { Separator } from '$lib/components/ui/separator'
	import { NotificationView } from '$lib/components/app'
	import { internalNotificationStore } from '$lib/client/internal-notification-store.svelte'
	import { authClient } from '$lib/client'

	let open = $state(false)
	let isLoading = $state(false)

	const session = authClient.useSession()
	const userId = $derived($session.data?.user?.userId)
	const unreadNotificationLabel = $derived(
		internalNotificationStore.unreadCount > 0
			? `Notifications, ${internalNotificationStore.unreadCount > 99 ? '99 or more' : internalNotificationStore.unreadCount} unread`
			: 'Notifications'
	)

	$effect(() => {
		internalNotificationStore.reset()
		if (userId === undefined) return

		const controller = new AbortController()
		void internalNotificationStore.refreshUnreadCount(controller.signal)
		const intervalId = setInterval(
			() => void internalNotificationStore.refreshUnreadCount(controller.signal),
			60000
		)

		return () => {
			controller.abort()
			clearInterval(intervalId)
		}
	})
</script>

{#if $session.data}
	<Popover.Root
		bind:open
		onOpenChange={async (value: boolean) => {
			if (value) {
				isLoading = true
				await internalNotificationStore.update()
				isLoading = false
			}
		}}
	>
		<Popover.Trigger
			class={buttonVariants({ variant: 'ghost', size: 'icon', class: 'relative' })}
			aria-label={unreadNotificationLabel}
		>
			<BellIcon />
			{#if internalNotificationStore.unreadCount > 0}
				<span class="pointer-events-none absolute -top-1 -right-1">
					<Badge class="h-4 min-w-4 p-0.5 font-mono tabular-nums" variant="destructive"
						>{internalNotificationStore.unreadCount > 99
							? '99+'
							: internalNotificationStore.unreadCount}</Badge
					>
				</span>
			{/if}
		</Popover.Trigger>
		<Popover.Content
			class="max-h-[min(24rem,calc(100dvh-2rem))] w-[min(24rem,calc(100vw-2rem))] overflow-auto"
		>
			<Popover.Header class="px-4 py-2">
				<Popover.Title>Notifications</Popover.Title>
			</Popover.Header>

			<Separator />

			{#if isLoading}
				<div class="p-4 text-center text-muted-foreground">Loading…</div>
			{:else if internalNotificationStore.notifications.length === 0}
				<div class="p-4 text-center text-muted-foreground">No unread notifications</div>
			{:else}
				<ul class="divide-y">
					{#each internalNotificationStore.notifications as notification (notification.notifiableEventId)}
						<NotificationView
							{notification}
							users={internalNotificationStore.users}
							threads={internalNotificationStore.threads}
							onChange={() => internalNotificationStore.update()}
							onNavigate={() => (open = false)}
						/>
					{/each}
				</ul>
			{/if}

			<Separator />
			<div class="p-2">
				<Button
					href={resolve('/(app)/current-user/notifications')}
					class="w-full"
					variant="link"
					onclick={() => (open = false)}
				>
					View all notifications
				</Button>
			</div>
		</Popover.Content>
	</Popover.Root>
{/if}
