<script lang="ts">
	import { CategoryView, Paginator } from '$lib/components/app'
	import { Separator } from '$lib/components/ui/separator'
	import { IconCategoryPlus, IconFolder } from '@tabler/icons-svelte'
	import type { PageProps } from './$types'
	import { Button, buttonVariants } from '$lib/components/ui/button'
	import { goto } from '$app/navigation'
	import { resolve } from '$app/paths'
	import { currentUser } from '$lib/client/current-user-state.svelte'

	let { data }: PageProps = $props()
</script>

{#if currentUser.user}
	<div class="flex items-center justify-between gap-x-2 px-4 sm:px-0">
		<div class="flex px-2 sm:px-0">
			<IconFolder class="mr-2 size-7 sm:mr-3 sm:size-8" />
			<h1 class="flex-1 text-xl font-bold sm:text-2xl">{data.forum.title}</h1>
		</div>
		<Button
			class={buttonVariants({ class: 'h-8' })}
			onclick={async () => {
				const path = resolve('/(app)/categories/create')
				const url = `${path}?forumId=${data.forum.forumId}`
				await goto(url)
			}}
		>
			<IconCategoryPlus class="size-4" />Create category</Button
		>
	</div>
{/if}

{#if data.forumData != null}
	<Paginator
		currentPage={data.currentPage}
		perPage={data.perPage}
		totalCount={data.categoryCount}
	/>
	<div class="mt-4 rounded-lg border px-4 py-2">
		{#each data.forumData.forumCategories as category, index}
			{@const latestPost = data.forumData.categoryLatestPosts.get(category.categoryId)}
			<CategoryView
				{category}
				threadCount={data.forumData.categoryThreadsCount.get(category.categoryId) ?? 0n}
				postCount={data.forumData.categoryPostsCount.get(category.categoryId) ?? 0n}
				{latestPost}
				users={data.forumData.users}
			/>
			{#if index < (data.forumData.forumCategories.length ?? 0) - 1}
				<Separator class="my-2" />
			{/if}
		{/each}
	</div>
{/if}
