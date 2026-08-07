<script lang="ts">
	import { invalidateAll } from '$app/navigation'
	import { NotificationView, Paginator } from '$lib/components/app'
	import { internalNotificationStore } from '$lib/client/internal-notification-store.svelte'
	import * as Empty from '$lib/components/ui/empty'
	import BellIcon from '@lucide/svelte/icons/bell'
	import type { PageProps } from './$types'
	import { PUBLIC_APP_NAME } from '$env/static/public'

	let { data }: PageProps = $props()

	async function handleNotificationChange() {
		await Promise.all([invalidateAll(), internalNotificationStore.refreshUnreadCount()])
	}
</script>

<svelte:head>
	<title>Notifications — {PUBLIC_APP_NAME}</title>
</svelte:head>

<section class="flex flex-col gap-4 px-4 sm:px-0">
	<div class="flex items-center gap-2">
		<BellIcon class="text-muted-foreground" />
		<h1 class="text-xl font-bold sm:text-2xl">Notifications</h1>
	</div>

	{#if data.notificationsData.totalCount > 0}
		<Paginator
			currentPage={data.currentPage}
			perPage={data.perPage}
			totalCount={data.notificationsData.totalCount}
		/>

		<ul class="divide-y overflow-hidden rounded-xl border bg-card">
			{#each data.notificationsData.notifications as notification (notification.notifiableEventId)}
				<NotificationView
					{notification}
					users={data.notificationsData.users}
					threads={data.notificationsData.threads}
					onChange={handleNotificationChange}
				/>
			{/each}
		</ul>
	{:else}
		<Empty.Root>
			<Empty.Header>
				<Empty.Media variant="icon"><BellIcon /></Empty.Media>
				<Empty.Title>No notifications</Empty.Title>
				<Empty.Description>Notifications about your activity will appear here.</Empty.Description>
			</Empty.Header>
		</Empty.Root>
	{/if}
</section>
