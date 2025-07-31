<script lang="ts">
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import { Textarea } from '$lib/components/ui/textarea'
	import { Button, buttonVariants } from '$lib/components/ui/button'
	import { Paginator, PostView } from '$lib/components/app'
	import type { PageProps } from './$types'
	import {
		createPost,
		createThreadSubscription,
		deleteThreadSubscription,
		getPostOrder,
		updatePost,
		type PostDto,
		ChannelType
	} from '$lib/utils/client'
	import { currentUser } from '$lib/client/current-user-state.svelte'
	import { goto } from '$app/navigation'
	import { IconBellOff, IconBellPlus, IconLoader2, IconPencil } from '@tabler/icons-svelte'
	import * as Dialog from '$lib/components/ui/dialog'
	import { Checkbox } from '$lib/components/ui/checkbox'
	import { ChannelTypeSchema } from '$lib/utils/client/schemas.gen'
	import { resolve } from '$app/paths'

	let creatingPost = $state(false)
	let { data }: PageProps = $props()

	let content: string | undefined = $state()
	let disabledPosting = $derived(
		currentUser.user == null || typeof content !== 'string' || content.trim().length < 1
	)

	let isSubscribed = $state(data.isSubscribed)
	let subscriptionLoading = $state(false)
	let subscriptionAbortController: AbortController | null = null

	let dialogOpen = $state(false)
	let selectedChannels = $state<ChannelType[]>([])

	let subscriptionButtonDisabled = $derived(subscriptionLoading || dialogOpen)

	const channelTypes = ChannelTypeSchema.enum.map((value: number, idx: number) => ({
		value,
		label: ChannelTypeSchema['x-enum-descriptions'][idx]
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
						path: { threadId: data.thread.threadId },
						auth: currentUser.user?.token,
						signal: subscriptionAbortController.signal
					})
				: await createThreadSubscription({
						path: { threadId: data.thread.threadId },
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

	async function onCreatePost() {
		if (disabledPosting) return
		creatingPost = true
		try {
			let postId
			if (!editedPost) {
				postId = (
					await createPost<true>({
						path: { threadId: data.thread.threadId },
						body: { content },
						auth: currentUser.user?.token
					})
				).data
			} else {
				await updatePost<true>({
					path: { threadId: editedPost.threadId, postId: editedPost.postId },
					body: { content, rowVersion: editedPost.rowVersion },
					auth: currentUser.user?.token
				})
				postId = editedPost.postId
			}
			const threadId = data.thread.threadId
			let postOrder = BigInt(
				(await getPostOrder<true>({ path: { threadId: data.thread.threadId, postId } })).data
			)
			const newPageIndex = postOrder / data.perPage + 1n
			content = undefined
			editedPost = undefined
			goto(
				`${resolve('/(app)/threads/[threadId=ThreadId]', { threadId })}?page=${newPageIndex}#post-${postId}`,
				{
					invalidateAll: true
				}
			)
		} finally {
			creatingPost = false
		}
	}

	let editedPost: PostDto | undefined = $state()

	const handleEdit = (post: PostDto) => {
		editedPost = post
		content = post.content
		const editor = document.getElementById('post-editor')
		if (editor) {
			editor.focus({ preventScroll: true })
			editor.scrollIntoView({ behavior: 'smooth' })
		}
	}
</script>

<Breadcrumb.Root>
	<Breadcrumb.List class="px-4 sm:px-0">
		<Breadcrumb.Item>
			<a href={resolve('/')}>Forums</a>
		</Breadcrumb.Item>
		<Breadcrumb.Separator />
		<Breadcrumb.Item>
			<a href={resolve('/(app)/forums/[forumId=ForumId]', { forumId: data.forum.forumId })}
				>{data.forum.title}</a
			>
		</Breadcrumb.Item>
		<Breadcrumb.Separator />
		<Breadcrumb.Item>
			<a
				href={resolve('/(app)/categories/[categoryId=CategoryId]', {
					categoryId: data.category.categoryId
				})}>{data.category.title}</a
			>
		</Breadcrumb.Item>
	</Breadcrumb.List>
</Breadcrumb.Root>

<div class="grid grid-cols-3 items-center">
	<div></div>
	<Paginator currentPage={data.currentPage} perPage={data.perPage} totalCount={data.postCount} />
	<div class="flex justify-end">
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
		{/if}
	</div>
</div>

{#if data.threadData}
	<section class="mt-4 grid gap-y-4">
		{#each data.threadData.threadPosts ?? [] as post}
			<PostView {post} author={data.threadData.users.get(post.createdBy)}>
				{#if currentUser.user?.id === post.createdBy}
					<Button onclick={() => handleEdit(post)} variant="ghost" class="size-8 cursor-pointer">
						<IconPencil />
					</Button>
				{/if}
			</PostView>
		{/each}
	</section>
{/if}

{#if currentUser.user}
	<Textarea
		id="post-editor"
		class="bg-muted/40 sm:bg-muted/0 mt-4 h-64 w-full border-0 sm:border"
		placeholder="Type your message here."
		disabled={creatingPost}
		bind:value={content}
	/>
	<div class="flex px-4 sm:px-0">
		<Button class="ml-auto mt-4" disabled={disabledPosting} onclick={onCreatePost}>Send</Button>
	</div>
{/if}

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
							class="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
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
