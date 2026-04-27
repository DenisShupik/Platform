<script lang="ts">
	import IconBellFilled from '~icons/tabler/bell-filled'
	import { buttonVariants, Button } from '$lib/components/ui/button'
	import { Badge } from '$lib/components/ui/badge'
	import * as Popover from '$lib/components/ui/popover'
	import { Separator } from '$lib/components/ui/separator'
	import { NotificationView } from '$lib/components/app'
	import { internalNotificationStore } from '$lib/client/internal-notification-store.svelte'
	import { authClient } from '$lib/client'
	import { getInternalNotificationCount } from '$lib/utils/client'

	let open = $state(false)
	let isLoading = $state(false)

	let totalCount = $derived($internalNotificationStore.totalCount)

	async function fetchCount() {
		try {
			totalCount = (
				await getInternalNotificationCount<true>({
					query: { isDelivered: false }
				})
			).data
		} catch (error) {
			console.error('fetchCount error: ', error)
		}
	}

	const session = authClient.useSession()

	$effect(() => {
		if (!$session.data) return
		fetchCount()
		const intervalId = setInterval(fetchCount, 60000)
		return () => clearInterval(intervalId)
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
		<Popover.Trigger class={buttonVariants({ variant: 'ghost', size: 'icon', class: 'relative' })}>
			<IconBellFilled class="size-6 text-primary" />
			{#if totalCount > 0}
				<span class="pointer-events-none absolute -top-1 -right-1">
					<Badge class="h-4 min-w-4 p-0.5 font-mono tabular-nums" variant="destructive"
						>{totalCount > 99 ? '99+' : totalCount}</Badge
					>
				</span>
			{/if}
		</Popover.Trigger>
		<Popover.Content class="max-h-96 w-96 overflow-auto">
			<div class="px-4 py-2">
				<h4 class="font-medium">Notifications</h4>
			</div>

			<Separator />

			{#if isLoading}
				<div class="p-4 text-center text-muted-foreground">Загрузка...</div>
			{:else if $internalNotificationStore.notifications.length === 0}
				<div class="p-4 text-center text-muted-foreground">Нет новых уведомлений</div>
			{:else}
				<ul class="divide-y">
					{#each $internalNotificationStore.notifications as notification (notification.notifiableEventId)}
						<NotificationView {notification} />
					{/each}
				</ul>
				<Separator />
				<div class="flex space-x-2 p-2">
					<Button class="flex-1" variant="link">Show all</Button>
				</div>
			{/if}
		</Popover.Content>
	</Popover.Root>
{/if}
