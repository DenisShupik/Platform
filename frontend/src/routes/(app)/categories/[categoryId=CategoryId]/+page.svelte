<script lang="ts">
	import { Paginator, ThreadView } from '$lib/components/app'
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import { Button, buttonVariants } from '$lib/components/ui/button'
	import { IconTextPlus } from '@tabler/icons-svelte'
	import type { PageProps } from './$types'
	import { goto } from '$app/navigation'
	import { resolve } from '$app/paths'

	let { data }: PageProps = $props()
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
	</Breadcrumb.List>
</Breadcrumb.Root>

<div class="flex items-center justify-between gap-x-2 px-4 sm:px-0">
	<h1 class="flex-1 text-xl font-bold sm:text-2xl">{data.category.title}</h1>
	<Button
		class={buttonVariants({ class: 'h-8' })}
		onclick={() => goto(resolve(`/(app)/threads/create?categoryId=${data.category.categoryId}`))}
	>
		<IconTextPlus class="size-4" />Create thread</Button
	>
</div>

{#if data.categoryData}
	<Paginator
		currentPage={data.currentPage}
		perPage={data.perPage}
		totalCount={data.categoryThreadsCount}
	/>
	<table class="mt-4 w-full table-auto border-collapse border">
		<colgroup>
			<col class="w-20" />
			<col />
			<col class="hidden w-24 md:table-column" />
			<col class="hidden w-52 md:table-column" />
		</colgroup>
		<tbody>
			{#each data.categoryData.categoryThreads as thread}
				<ThreadView
					{thread}
					postCount={data.categoryData.threadsPostsCount.get(thread.threadId) ?? 0n}
					latestPost={data.categoryData.threadsPostsLatest.get(thread.threadId)}
					users={data.categoryData.users}
				/>
			{/each}
		</tbody>
	</table>
{/if}
