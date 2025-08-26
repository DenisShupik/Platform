<script lang="ts">
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import { Textarea } from '$lib/components/ui/textarea'
	import { Button } from '$lib/components/ui/button'
	import type { PageProps } from './$types'
	import { createPost } from '$lib/utils/client'
	import { currentUser } from '$lib/client/current-user-state.svelte'
	import { goto } from '$app/navigation'
	import { resolve } from '$app/paths'

	let creatingPost = $state(false)
	let { data }: PageProps = $props()

	let content: string | undefined = $state()

	let disabledPosting = $derived(
		currentUser.user == null || typeof content !== 'string' || content.trim().length < 1
	)

	async function onCreatePost() {
		if (disabledPosting) return
		creatingPost = true
		try {
			await createPost({
				path: { threadId: data.thread.threadId },
				body: { content },
				auth: currentUser.user?.token
			})
			const threadId = data.thread.threadId
			content = undefined
			goto(resolve('/(app)/threads/[threadId=ThreadId]', { threadId }), { invalidateAll: true })
		} finally {
			creatingPost = false
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

{#if currentUser.user}
	<Textarea
		id="post-editor"
		class="bg-muted/40 sm:bg-muted/0 mt-4 h-64 w-full border-0 sm:border"
		placeholder="Type your message here."
		disabled={creatingPost}
		bind:value={content}
	/>
	<div class="flex px-4 sm:px-0">
		<Button class="mt-4 ml-auto" disabled={disabledPosting} onclick={onCreatePost}
			>Опубликовать</Button
		>
	</div>
{/if}
