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
		type PostDto
	} from '$lib/utils/client'
	import { authStore, currentUser } from '$lib/client/auth-state.svelte'
	import { goto } from '$app/navigation'
	import { IconBellOff, IconBellPlus, IconLoader2, IconPencil } from '@tabler/icons-svelte'

	let creatingPost = $state(false)
	let { data }: PageProps = $props()

	let content: string | undefined = $state()

	let disabledPosting = $derived(
		$currentUser == null || typeof content !== 'string' || content.trim().length < 1
	)

	let isSubscribed = $state(data.isSubscribed)
	let subscriptionLoading = $state(false)
	let subscriptionAbortController: AbortController | null = null

	async function handleSubscription() {
		if (subscriptionLoading && subscriptionAbortController) {
			subscriptionAbortController.abort()
			subscriptionLoading = false
			return
		}
		subscriptionLoading = true
		subscriptionAbortController = new AbortController()
		try {
			if (!isSubscribed) {
				await createThreadSubscription({
					path: { threadId: data.thread.threadId },
					auth: $authStore.token,
					signal: subscriptionAbortController.signal
				})
				isSubscribed = true
			} else {
				await deleteThreadSubscription({
					path: { threadId: data.thread.threadId },
					auth: $authStore.token,
					signal: subscriptionAbortController.signal
				})
				isSubscribed = false
			}
		} catch (e) {
			// handle error if needed
		} finally {
			subscriptionLoading = false
			subscriptionAbortController = null
		}
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
						auth: $authStore.token
					})
				).data
			} else {
				await updatePost<true>({
					path: { threadId: editedPost.threadId, postId: editedPost.postId },
					body: { content, rowVersion: editedPost.rowVersion },
					auth: $authStore.token
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
			goto('/threads/' + threadId + '?page=' + newPageIndex + '#post-' + postId, {
				invalidateAll: true
			})
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
			<a href="/">Forums</a>
		</Breadcrumb.Item>
		<Breadcrumb.Separator />
		<Breadcrumb.Item>
			<a href={`/forums/${data.forum.forumId}`}>{data.forum.title}</a>
		</Breadcrumb.Item>
		<Breadcrumb.Separator />
		<Breadcrumb.Item>
			<a href={`/categories/${data.category.categoryId}`}>{data.category.title}</a>
		</Breadcrumb.Item>
	</Breadcrumb.List>
</Breadcrumb.Root>

<div class="grid grid-cols-3 items-center">
	<div></div>
	<Paginator currentPage={data.currentPage} perPage={data.perPage} totalCount={data.postCount} />
	<div class="flex justify-end">
		{#if $currentUser}
			<Button class={buttonVariants({ class: 'h-8' })} onclick={handleSubscription}>
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
				{#if $currentUser?.id === post.createdBy}
					<Button
						onclick={() => {
							handleEdit(post)
						}}
						variant="ghost"
						class="size-8 cursor-pointer"><IconPencil /></Button
					>
				{/if}
			</PostView>
		{/each}
	</section>
{/if}

{#if $currentUser}
	<Textarea
		id="post-editor"
		class="bg-muted/40 sm:bg-muted/0 mt-4 h-64 w-full border-0 sm:border"
		placeholder="Type your message here."
		disabled={creatingPost}
		bind:value={content}
	/>
	<div class="flex px-4 sm:px-0">
		<Button class="ml-auto mt-4" disabled={disabledPosting} onclick={onCreatePost}>Отправить</Button
		>
	</div>
{/if}
