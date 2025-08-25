<script lang="ts">
	import { IconBellFilled } from '@tabler/icons-svelte'
	import { buttonVariants, Button } from '$lib/components/ui/button'
	import { Badge } from '$lib/components/ui/badge'
	import { currentUser } from '$lib/client/current-user-state.svelte'
	import * as Popover from '$lib/components/ui/popover'
	import { Separator } from '$lib/components/ui/separator'
	import { NotificationView } from '$lib/components/app'
	import { internalNotificationStore } from '$lib/client/internal-notification-store.svelte'
	import { getInternalNotificationCount } from '$lib/utils/client'

	let open = $state(false)
	let isLoading = $state(false)

	let totalCount = $derived($internalNotificationStore.totalCount)

	async function fetchCount() {
		try {
			const result = await getInternalNotificationCount<true>({
				query: { isDelivered: false },
				auth: currentUser.user?.token
			})

			totalCount = BigInt(result.data)
		} catch (error) {
			console.error('Ошибка при получении количества уведомлений:', error)
		}
	}

	$effect(() => {
		if (!currentUser.user) return
		fetchCount()
		const intervalId = setInterval(fetchCount, 60000)
		return () => clearInterval(intervalId)
	})
</script>

{#if currentUser.user}
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
			<IconBellFilled class="text-primary size-6" />
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
				<div class="text-muted-foreground p-4 text-center">Загрузка...</div>
			{:else if $internalNotificationStore.notifications.length === 0}
				<div class="text-muted-foreground p-4 text-center">Нет новых уведомлений</div>
			{:else}
				<ul class="divide-y">
					{#each $internalNotificationStore.notifications as notification}
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
