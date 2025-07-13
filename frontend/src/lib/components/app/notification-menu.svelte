<script lang="ts">
	import { IconBellFilled } from '@tabler/icons-svelte'
	import { buttonVariants, Button } from '$lib/components/ui/button'
	import { Badge } from '$lib/components/ui/badge'
	import { authStore, currentUser } from '$lib/client/auth-state.svelte'
	import {
		ChannelType,
		getUserNotification,
		getUserNotificationCount,
		type InternalUserNotificationsDto
	} from '$lib/utils/client'
	import * as Popover from '$lib/components/ui/popover'
	import { Separator } from '$lib/components/ui/separator'
	import { route } from '$lib/ROUTES'

	let open = $state(false)
	let count: number = $state(0)
	let notifications: InternalUserNotificationsDto | undefined = $state()
	let loading = $state(false)

	async function fetchNotificationCount() {
		try {
			count = (
				await getUserNotificationCount<true>({
					query: { isDelivered: false, channel: ChannelType.INTERNAL },
					auth: $authStore.token
				})
			).data
		} catch (error) {
			console.error('Ошибка при получении количества уведомлений:', error)
		}
	}

	async function fetchNotifications() {
		loading = true
		try {
			const res = await getUserNotification<true>({
				query: { channel: ChannelType.INTERNAL },
				auth: $authStore.token
			})
			notifications = res.data ?? []
		} catch (error) {
			console.error('Ошибка при получении уведомлений:', error)
		} finally {
			loading = false
		}
	}

	$effect(() => {
		if (!$currentUser) return
		fetchNotificationCount()
		const intervalId = setInterval(fetchNotificationCount, 60000)
		return () => clearInterval(intervalId)
	})
</script>

{#if $currentUser}
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
		<Popover.Content class="max-h-96 w-80 overflow-auto">
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
					{#each notifications?.notifications ?? [] as n}
						{@const author = notifications?.users[n.payload.createdBy]}
						{@const threadTitle = notifications.threads[n.payload.threadId]}
						<li class="hover:bg-accent flex cursor-pointer flex-col p-3 font-medium">
							<div class="flex items-center space-x-1">
								<span>{author ?? '—'}</span>
								<span>posted to</span>
								<a
									class="text-blue-600 hover:underline"
									href={route('/threads/[threadId=threadId]', { threadId: n.payload.threadId })}
									>{threadTitle ?? '—'}</a
								>
							</div>
							<div class="text-muted-foreground mt-1 text-xs">
								{new Date(n.occurredAt).toLocaleString()}
							</div>
						</li>
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
