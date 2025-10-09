<script lang="ts">
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import { Button } from '$lib/components/ui/button'
	import { Paginator, PostEditor, PostView, ThreadSubscriptionButton } from '$lib/components/app'
	import type { PageProps } from './$types'
	import type { PostDto } from '$lib/utils/client'
	import { IconPencil } from '@tabler/icons-svelte'
	import { resolve } from '$app/paths'

	let { data }: PageProps = $props()

	let startPostIndex = $derived((data.currentPage - 1n) * data.perPage + 1n)
	let isSubscribed = $state(data.isSubscribed ?? false)
	let editedPost = $state<PostDto | undefined>()
	// TODO: теперь есть функционал анонимных постов, добавить поддержку
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
		<ThreadSubscriptionButton threadId={data.thread.threadId} bind:isSubscribed />
	</div>
</div>

{#if data.threadData}
	<section class="mt-4 grid gap-y-4">
		{#each data.threadData.threadPosts ?? [] as post, index}
			<PostView
				{post}
				index={startPostIndex + BigInt(index)}
				author={post.createdBy != null ? data.threadData.users.get(post.createdBy) : undefined}
			>
				{#if data.session?.user?.userId != null && post.createdBy != null && data.session.user.userId === post.createdBy}
					<Button onclick={() => (editedPost = post)} variant="ghost" class="size-8 cursor-pointer">
						<IconPencil />
					</Button>
				{/if}
			</PostView>
		{/each}
	</section>
{/if}

{#if data.session}
	<PostEditor threadId={data.thread.threadId} perPage={data.perPage} bind:editedPost />
{/if}
