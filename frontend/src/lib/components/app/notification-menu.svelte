<script lang="ts">
	import { IconBellFilled } from '@tabler/icons-svelte'
	import { buttonVariants, Button } from '$lib/components/ui/button'
	import { Badge } from '$lib/components/ui/badge'
	import { currentUser } from '$lib/client/current-user-state.svelte'
	import {
		ChannelType,
		GetInternalUserNotificationQuerySortEnum,
		getUserNotification,
		getUserNotificationCount,
		type InternalUserNotificationsDto
	} from '$lib/utils/client'
	import * as Popover from '$lib/components/ui/popover'
	import { Separator } from '$lib/components/ui/separator'
	import { NotificationView } from '$lib/components/app'

	let open = $state(false)
	let count: number = $state(0)
	let notifications: InternalUserNotificationsDto | undefined = $state()
	let loading = $state(false)

	async function fetchNotificationCount() {
		try {
			count = (
				await getUserNotificationCount<true>({
					query: { isDelivered: false, channel: ChannelType.INTERNAL },
					auth: currentUser.user?.token
				})
			).data
		} catch (error) {
			console.error('Ошибка при получении количества уведомлений:', error)
		}
	}

	async function fetchNotifications() {
		loading = true
		try {
			const result = await getUserNotification<true>({
				query: {
					isDelivered: false,
					sort: [GetInternalUserNotificationQuerySortEnum.OCCURRED_AT_ASC]
				},
				auth: currentUser.user?.token
			})
			notifications = result.data ?? []
		} catch (error) {
			console.error('Ошибка при получении уведомлений:', error)
		} finally {
			loading = false
		}
	}

	async function handleNotificationUpdate() {
		await Promise.all([fetchNotificationCount(), fetchNotifications()])
	}

	$effect(() => {
		if (!currentUser.user) return
		fetchNotificationCount()
		const intervalId = setInterval(fetchNotificationCount, 60000)
		return () => clearInterval(intervalId)
	})
</script>

{#if currentUser.user}
	<Popover.Root
		bind:open
		onOpenChange={(o: boolean) => {
			if (o) fetchNotifications()
		}}
	>
		<Popover.Trigger class={buttonVariants({ variant: 'ghost', size: 'icon', class: 'relative' })}>
			<IconBellFilled class="text-primary size-6" />
			{#if count > 0}
				<span class="pointer-events-none absolute -right-1 -top-1">
					<Badge class="h-4 min-w-4 p-0.5 font-mono tabular-nums" variant="destructive"
						>{count > 99 ? '99+' : count}</Badge
					>
				</span>
			{/if}
		</Popover.Trigger>
		<Popover.Content class="max-h-96 w-96 overflow-auto">
			<div class="px-4 py-2">
				<h4 class="font-medium">Notifications</h4>
			</div>

			<Separator />

			{#if loading}
				<div class="text-muted-foreground p-4 text-center">Загрузка...</div>
			{:else if notifications?.notifications.length === 0}
				<div class="text-muted-foreground p-4 text-center">Нет новых уведомлений</div>
			{:else}
				<ul class="divide-y">
					{#each notifications?.notifications ?? [] as notification}
						<NotificationView {notification} {notifications} onupdated={handleNotificationUpdate} />
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
