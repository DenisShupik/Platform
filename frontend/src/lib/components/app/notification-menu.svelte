<script lang="ts">
	import {
		IconBellFilled,
		IconCheck,
		IconClockFilled,
		IconEyeCheck,
		IconTrash
	} from '@tabler/icons-svelte'
	import { buttonVariants, Button } from '$lib/components/ui/button'
	import { Badge } from '$lib/components/ui/badge'
	import { authStore, currentUser } from '$lib/client/auth-state.svelte'
	import {
		ChannelType,
		GetInternalUserNotificationQuerySortEnum,
		getUserNotification,
		getUserNotificationCount,
		type InternalUserNotificationsDto
	} from '$lib/utils/client'
	import * as Popover from '$lib/components/ui/popover'
	import { Separator } from '$lib/components/ui/separator'
	import { route } from '$lib/ROUTES'
	import * as Avatar from '$lib/components/ui/avatar'
	import { PUBLIC_AVATAR_URL } from '$env/static/public'
	import { formatTimestamp } from '$lib/utils/formatTimestamp'

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
				query: {
					sort: [
						GetInternalUserNotificationQuerySortEnum.OCCURRED_AT_ASC,
						GetInternalUserNotificationQuerySortEnum.DELIVERED_AT_ASC
					]
				},
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
					{#each notifications?.notifications ?? [] as n}
						{@const author = notifications?.users[n.payload.createdBy]}
						{@const threadTitle = notifications?.threads[n.payload.threadId]}
						<li class="hover:bg-muted/50 flex cursor-pointer flex-col p-3 font-medium">
							<div class="flex flex-row space-x-4">
								<Avatar.Root class="size-8 place-self-center">
									<Avatar.Image src="{PUBLIC_AVATAR_URL}/{n.payload.createdBy}" alt="@shadcn" />
									<Avatar.Fallback>{author}</Avatar.Fallback>
								</Avatar.Root>
								<div class="flex flex-1 flex-col justify-center space-y-1">
									<p>
										<span>{author ?? '—'}</span>
										<span>posted to</span>
										<a
											class="text-blue-600 hover:underline"
											href={route('/threads/[threadId=ThreadId]', { threadId: n.payload.threadId })}
											>{threadTitle ?? '—'}</a
										>
									</p>
									<p class="text-muted-foreground flex items-center gap-x-1 text-xs">
										<IconClockFilled class="inline size-3" /><time
											>{formatTimestamp(n.occurredAt)}</time
										>
									</p>
								</div>
								<div class="flex flex-col space-y-2 place-self-center">
									<Button variant="outline" size="icon" class="size-6 cursor-pointer">
										<IconEyeCheck class="shape-crisp-edges"/>
									</Button>
									<Button variant="destructive" size="icon" class="size-6 cursor-pointer">
										<IconTrash />
									</Button>
								</div>
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
