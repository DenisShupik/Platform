<script lang="ts">
	import { withApiLocale } from '$lib/client/api-options'
	import { Button, buttonVariants } from '$lib/components/ui/button'
	import { Checkbox } from '$lib/components/ui/checkbox'
	import * as Dialog from '$lib/components/ui/dialog'
	import IconBellOff from '~icons/tabler/bell-off'
	import IconBellPlus from '~icons/tabler/bell-plus'
	import {
		createThreadSubscription,
		deleteThreadSubscription,
		ChannelType,
		type ThreadId
	} from '$lib/utils/client'
	import { ChannelTypeSchema } from '$lib/utils/client/schemas.gen'
	import { authClient } from '$lib/client'
	import { Spinner } from '$lib/components/ui/spinner'
	import ButtonTitle from './button-title.svelte'
	import * as m from '$lib/paraglide/messages'
	import XIcon from '@lucide/svelte/icons/x'

	let {
		threadId,
		isSubscribed = $bindable(),
		onSubscriptionChange = () => {}
	}: {
		threadId: ThreadId
		isSubscribed: boolean
		onSubscriptionChange?: (isSubscribed: boolean) => void | Promise<void>
	} = $props()

	let subscriptionLoading = $state(false)
	let subscriptionAbortController: AbortController | null = null
	let dialogOpen = $state(false)
	let selectedChannels = $state<ChannelType[]>([])

	let subscriptionButtonDisabled = $derived(subscriptionLoading || dialogOpen)

	const channelTypes = ChannelTypeSchema.enum.map((value) => ({
		value: value as ChannelType,
		label:
			value === ChannelType.INTERNAL
				? m.subscription_channel_internal()
				: m.subscription_channel_email()
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

		const userId = $session.data?.user?.userId
		if (!userId) return

		if (!isSubscribed && selectedChannels.length === 0) return

		subscriptionLoading = true
		subscriptionAbortController = new AbortController()

		try {
			const result = isSubscribed
				? await deleteThreadSubscription<false>(
						withApiLocale({
							path: { userId, threadId },
							signal: subscriptionAbortController.signal,
							throwOnError: false
						})
					)
				: await createThreadSubscription<false>(
						withApiLocale({
							path: { userId, threadId },
							body: { channels: selectedChannels },
							signal: subscriptionAbortController.signal,
							throwOnError: false
						})
					)

			if (result?.error) {
				console.error('Subscription action failed:', result.error)
				return
			}

			isSubscribed = !isSubscribed
			void onSubscriptionChange(isSubscribed)
			dialogOpen = false
			selectedChannels = []
		} catch (error) {
			console.error('Subscription action failed:', error)
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

	const session = authClient.useSession()
</script>

{#if $session.data}
	<Button
		class={buttonVariants({ class: 'h-8' })}
		onclick={() => (dialogOpen = true)}
		disabled={subscriptionButtonDisabled}
	>
		{#if subscriptionLoading}
			<Spinner />
		{:else if isSubscribed}
			<IconBellOff class="size-4" />
		{:else}
			<IconBellPlus class="size-4" />
		{/if}
		<ButtonTitle
			>{isSubscribed ? m.subscription_unsubscribe() : m.subscription_subscribe()}</ButtonTitle
		>
	</Button>

	<!-- Shared dialog for subscribing and unsubscribing. -->
	<Dialog.Root bind:open={dialogOpen} onOpenChange={(open) => !open && closeDialog()}>
		<Dialog.Content class="sm:max-w-106.25" showCloseButton={false}>
			<Dialog.Close>
				{#snippet child({ props })}
					<Button {...props} variant="ghost" size="icon-sm" class="absolute top-4 right-4">
						<XIcon />
						<span class="sr-only">{m.common_close()}</span>
					</Button>
				{/snippet}
			</Dialog.Close>
			<Dialog.Header>
				<Dialog.Title>
					{isSubscribed ? m.subscription_confirm_unsubscribe() : m.subscription_select_channels()}
				</Dialog.Title>
				<Dialog.Description>
					{isSubscribed
						? m.subscription_unsubscribe_description()
						: m.subscription_subscribe_description()}
				</Dialog.Description>
			</Dialog.Header>

			{#if !isSubscribed}
				<div class="grid gap-4 py-4">
					{#each channelTypes as channel (channel.value)}
						<div class="flex items-center gap-2">
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
						<Spinner class="mr-2" />
						{m.common_cancel()}
					{:else}
						{isSubscribed ? m.subscription_unsubscribe() : m.subscription_subscribe()}
					{/if}
				</Button>
			</Dialog.Footer>
		</Dialog.Content>
	</Dialog.Root>
{/if}
