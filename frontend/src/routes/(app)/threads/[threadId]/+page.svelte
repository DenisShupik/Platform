<script lang="ts">
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import { Textarea } from '$lib/components/ui/textarea'
	import { Button } from '$lib/components/ui/button'
	import { Paginator, PostView } from '$lib/components/app'
	import type { PageProps } from './$types'
	import {
		createPost,
		getPostOrder,
		getThreadsPostsCount,
		updatePost,
		type PostDto
	} from '$lib/utils/client'
	import { authStore, currentUser } from '$lib/client/auth-state.svelte'
	import { goto } from '$app/navigation'
	import { IconPencil } from '@tabler/icons-svelte'

	let creatingPost = $state(false)
	let { data }: PageProps = $props()

	let content: string | undefined = $state()

	let disabledPosting = $derived(
		$currentUser == null || typeof content !== 'string' || content.trim().length < 1
	)

	async function onCreatePost() {
		if (disabledPosting) return
		creatingPost = true
		try {
			let postId
			if (!editedPost) {
				postId = (
					await createPost({
						path: { threadId: data.thread.threadId },
						body: { content },
						auth: $authStore.token
					})
				).data
			} else {
				await updatePost({
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
		document.getElementById('post-editor')?.focus({ preventScroll: true })
		document.getElementById('post-editor')?.scrollIntoView({ behavior: 'smooth' })
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

<Paginator currentPage={data.currentPage} perPage={data.perPage} totalCount={data.postCount} />

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
		class="bg-muted/40 sm:border-1 sm:bg-muted/0 mt-4 h-64 w-full border-0"
		placeholder="Type your message here."
		disabled={creatingPost}
		bind:value={content}
	/>
	<div class="flex px-4 sm:px-0">
		<Button class="ml-auto mt-4" disabled={disabledPosting} onclick={onCreatePost}>Отправить</Button
		>
	</div>
{/if}
