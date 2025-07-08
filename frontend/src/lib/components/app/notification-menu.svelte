<script lang="ts">
	import { IconBellFilled } from '@tabler/icons-svelte'
	import { Button } from '$lib/components/ui/button'
	import { Badge } from '$lib/components/ui/badge'
	import { authStore, currentUser } from '$lib/client/auth-state.svelte'
	import { ChannelType, getUserNotificationCount } from '$lib/utils/client'

	let count: bigint = $state(0n)

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

	$effect(() => {
		if (!$currentUser) return
		fetchNotificationCount()
		const intervalId = setInterval(fetchNotificationCount, 60000)
		return () => clearInterval(intervalId)
	})
</script>

{#if $currentUser}
	<Button variant="ghost" size="icon" class="relative">
		<IconBellFilled class="text-primary size-6" />
		{#if count > 0n}
			<span class="pointer-events-none absolute -right-1 -top-1">
				<Badge class="h-4 min-w-4 p-0.5 font-mono tabular-nums" variant="destructive"
					>{count > 99n ? '99+' : count}</Badge
				>
			</span>
		{/if}
	</Button>
{/if}
