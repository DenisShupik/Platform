<script lang="ts">
	import { resolve } from '$app/paths'
	import { Button } from '$lib/components/ui/button'
	import * as Card from '$lib/components/ui/card'
	import type { ThreadDto } from '$lib/utils/client'
	import ExternalLinkIcon from '@lucide/svelte/icons/external-link'
	import ThreadSubscriptionButton from './thread-subscription-button.svelte'

	let {
		thread,
		onUnsubscribe
	}: {
		thread: ThreadDto
		onUnsubscribe: () => void | Promise<void>
	} = $props()

	let isSubscribed = $state(true)

	function handleSubscriptionChange(subscribed: boolean) {
		if (!subscribed) void onUnsubscribe()
	}
</script>

{#if isSubscribed}
	<Card.Root size="sm">
		<Card.Header>
			<Card.Description>Watched thread</Card.Description>
			<Card.Title class="truncate">
				<a href={resolve(`/threads/${thread.threadId}`)} class="hover:underline">
					{thread.title}
				</a>
			</Card.Title>
		</Card.Header>
		<Card.Footer class="justify-between gap-4 border-t">
			<ThreadSubscriptionButton
				threadId={thread.threadId}
				bind:isSubscribed
				onSubscriptionChange={handleSubscriptionChange}
			/>
			<Button href={resolve(`/threads/${thread.threadId}`)} variant="outline" size="sm">
				Open thread
				<ExternalLinkIcon data-icon="inline-end" />
			</Button>
		</Card.Footer>
	</Card.Root>
{/if}
