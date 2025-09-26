<script lang="ts">
	import { Button, buttonVariants } from '$lib/components/ui/button'
	import { Checkbox } from '$lib/components/ui/checkbox'
	import * as Dialog from '$lib/components/ui/dialog'
	import { IconBellOff, IconBellPlus, IconLoader2 } from '@tabler/icons-svelte'
	import { currentUser } from '$lib/client/current-user-state.svelte'
	import {
		createThreadSubscription,
		deleteThreadSubscription,
		type ChannelType
	} from '$lib/utils/client'
	import { ChannelTypeSchema } from '$lib/utils/client/schemas.gen'

	let {
		threadId,
		isSubscribed = $bindable()
	}: {
		threadId: string
		isSubscribed: boolean
	} = $props()

	let subscriptionLoading = $state(false)
	let subscriptionAbortController: AbortController | null = null
	let dialogOpen = $state(false)
	let selectedChannels = $state<ChannelType[]>([])

	let subscriptionButtonDisabled = $derived(subscriptionLoading || dialogOpen)

   
    // var labels = ChannelTypeSchema['x-enum-descriptions'];
	let labels = ChannelTypeSchema["x-enum-varnames"];

	const channelTypes = ChannelTypeSchema.enum.map((value: number, idx: number) => ({
		value,
		label: labels[idx]
	}))

	function cancelRequest() {
		subscriptionAbortController?.abort()
		subscriptionAbortController = null
		subscriptionLoading = false
	}

	function closeDialog() {
		cancelRequest()
		dialogOpen = false
		selectedChannels = []
	}

	async function handleSubscriptionAction() {
		if (subscriptionLoading) {
			cancelRequest()
			return
		}

		if (!isSubscribed && selectedChannels.length === 0) return

		subscriptionLoading = true
		subscriptionAbortController = new AbortController()

		try {
			const result = isSubscribed
				? await deleteThreadSubscription({
						path: { threadId },
						auth: currentUser.user?.token,
						signal: subscriptionAbortController.signal
					})
				: await createThreadSubscription({
						path: { threadId },
						body: { channels: selectedChannels },
						auth: currentUser.user?.token,
						signal: subscriptionAbortController.signal
					})

			// Проверка на ошибку в ответе
			if (result?.error) {
				console.error('Subscription action failed:', result.error)
				return
			}

			// Закрываем диалог и сбрасываем выбранные каналы только при успехе
			isSubscribed = !isSubscribed
			dialogOpen = false
			selectedChannels = []
		} catch (error) {
			// В случае ошибки состояние isSubscribed остается неизменным
			console.error('Subscription action failed:', error)
			// Можно добавить уведомление об ошибке для пользователя
		} finally {
			subscriptionLoading = false
			subscriptionAbortController = null
		}
	}

	function toggleChannel(channelValue: ChannelType) {
		selectedChannels = selectedChannels.includes(channelValue)
			? selectedChannels.filter((c) => c !== channelValue)
			: [...selectedChannels, channelValue]
	}
</script>

{#if currentUser.user}
	<Button
		class={buttonVariants({ class: 'h-8' })}
		onclick={() => (dialogOpen = true)}
		disabled={subscriptionButtonDisabled}
	>
		{#if subscriptionLoading}
			<IconLoader2 class="size-4 animate-spin" />
		{:else if isSubscribed}
			<IconBellOff class="size-4" />
		{:else}
			<IconBellPlus class="size-4" />
		{/if}
		{isSubscribed ? 'Unsubscribe' : 'Subscribe'}
	</Button>

	<!-- Единый диалог для подписки/отписки -->
	<Dialog.Root bind:open={dialogOpen} onOpenChange={(open) => !open && closeDialog()}>
		<Dialog.Content class="sm:max-w-[425px]">
			<Dialog.Header>
				<Dialog.Title>
					{isSubscribed ? 'Confirm unsubscribe' : 'Select notification channels'}
				</Dialog.Title>
				<Dialog.Description>
					{isSubscribed
						? 'Are you sure you want to unsubscribe from notifications for this thread?'
						: 'Choose how you want to receive notifications about new posts in this thread.'}
				</Dialog.Description>
			</Dialog.Header>

			{#if !isSubscribed}
				<div class="grid gap-4 py-4">
					{#each channelTypes as channel}
						<div class="flex items-center space-x-2">
							<Checkbox
								id={`channel-${channel.value}`}
								checked={selectedChannels.includes(channel.value)}
								onCheckedChange={() => toggleChannel(channel.value)}
								disabled={subscriptionLoading}
							/>
							<label
								for={`channel-${channel.value}`}
								class="text-sm leading-none font-medium peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
							>
								{channel.label}
							</label>
						</div>
					{/each}
				</div>
			{/if}

			<Dialog.Footer>
				<Button
					onclick={handleSubscriptionAction}
					disabled={!subscriptionLoading && !isSubscribed && selectedChannels.length === 0}
					variant={subscriptionLoading ? 'outline' : isSubscribed ? 'destructive' : 'default'}
				>
					{#if subscriptionLoading}
						<IconLoader2 class="mr-2 size-4 animate-spin" />
						Cancel
					{:else}
						{isSubscribed ? 'Unsubscribe' : 'Subscribe'}
					{/if}
				</Button>
			</Dialog.Footer>
		</Dialog.Content>
	</Dialog.Root>
{/if}
