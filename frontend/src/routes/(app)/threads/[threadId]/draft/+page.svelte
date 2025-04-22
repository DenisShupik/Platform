<script lang="ts">
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import { Textarea } from '$lib/components/ui/textarea'
	import { Button } from '$lib/components/ui/button'
	import type { PageProps } from './$types'
	import { createPost } from '$lib/utils/client'
	import { authStore, currentUser } from '$lib/client/auth-state.svelte'
	import { goto } from '$app/navigation'

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
			await createPost({
				path: { threadId: data.thread.threadId },
				body: { content },
				auth: $authStore.token
			})
			const threadId = data.thread.threadId
			content = undefined
			goto('/threads/' + threadId, { invalidateAll: true })
		} finally {
			creatingPost = false
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

{#if $currentUser}
	<Textarea
		id="post-editor"
		class="bg-muted/40 sm:border-1 sm:bg-muted/0 mt-4 h-64 w-full border-0"
		placeholder="Type your message here."
		disabled={creatingPost}
		bind:value={content}
	/>
	<div class="flex px-4 sm:px-0">
		<Button class="ml-auto mt-4" disabled={disabledPosting} onclick={onCreatePost}>Опубликовать</Button
		>
	</div>
{/if}
