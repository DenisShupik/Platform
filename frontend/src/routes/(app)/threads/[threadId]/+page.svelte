<script lang="ts">
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import { Textarea } from '$lib/components/ui/textarea'
	import { Button } from '$lib/components/ui/button'
	import { Paginator, PostView } from '$lib/components/app'
	import type { PageProps } from './$types'
	import { createPost, getThreadPostsCount } from '$lib/utils/client'
	import { authStore, currentUser } from '$lib/client/auth-state.svelte'
	import { goto } from '$app/navigation'

	let { data }: PageProps = $props()

	let content: string | undefined = $state()

	let disabledPosting = $derived(
		$currentUser == null || typeof content !== 'string' || content.trim().length < 1
	)

	async function onCreatePost() {
		if (disabledPosting) return
		await createPost({
			path: { threadId: data.thread.threadId },
			body: { content },
			auth: $authStore.token
		})
		const threadId = data.thread.threadId
		let postCount = BigInt(
			(await getThreadPostsCount<true>({ path: { threadIds: [threadId] } })).data[`${threadId}`]
		)
		const newPageIndex = postCount / data.perPage + 1n

		goto('/threads/' + threadId + '?page=' + newPageIndex, { invalidateAll: true })
	}
</script>

<Breadcrumb.Root>
	<Breadcrumb.List>
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

<section class="mt-4 grid gap-y-4">
	{#each data.threadPosts ?? [] as post}
		<PostView {post} author={data.users.get(post.createdBy)} />
	{/each}
</section>

{#if $currentUser}
	<Textarea class="mt-4 h-64 w-full " placeholder="Type your message here." bind:value={content} />
	<div class="flex">
		<Button class="ml-auto mt-4" disabled={disabledPosting} onclick={onCreatePost}>Отправить</Button
		>
	</div>
{/if}
