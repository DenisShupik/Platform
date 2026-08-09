<script lang="ts">
	import { invalidateAll } from '$app/navigation'
	import { Paginator, WatchedThreadCard } from '$lib/components/app'
	import * as Empty from '$lib/components/ui/empty'
	import EyeIcon from '@lucide/svelte/icons/eye'
	import type { PageProps } from './$types'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import * as m from '$lib/paraglide/messages'

	let { data }: PageProps = $props()

	async function handleUnsubscribe() {
		await invalidateAll()
	}
</script>

<svelte:head>
	<title>{m.watched()} — {PUBLIC_APP_NAME}</title>
</svelte:head>

<section class="flex flex-col gap-4">
	<div class="flex items-center gap-2">
		<EyeIcon class="text-muted-foreground" />
		<h1 class="text-xl font-bold sm:text-2xl">{m.watched()}</h1>
	</div>

	{#if data.threadSubscriptionsData.totalCount > 0}
		<Paginator
			currentPage={data.currentPage}
			perPage={data.perPage}
			totalCount={data.threadSubscriptionsData.totalCount}
		/>

		{#if data.threadSubscriptionsData.items.length > 0}
			<div class="flex flex-col gap-4">
				{#each data.threadSubscriptionsData.items as thread (thread.threadId)}
					<WatchedThreadCard {thread} onUnsubscribe={handleUnsubscribe} />
				{/each}
			</div>
		{/if}
	{:else}
		<Empty.Root>
			<Empty.Header>
				<Empty.Media variant="icon"><EyeIcon /></Empty.Media>
				<Empty.Title>{m.watched_none()}</Empty.Title>
				<Empty.Description>{m.watched_empty_description()}</Empty.Description>
			</Empty.Header>
		</Empty.Root>
	{/if}
</section>
