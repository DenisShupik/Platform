<script lang="ts">
	import { ButtonTitle, CategoryView, Paginator } from '$lib/components/app'
	import { Separator } from '$lib/components/ui/separator'
	import IconCategoryPlus from '~icons/tabler/category-plus'
	import type { PageProps } from './$types'
	import { Button, buttonVariants } from '$lib/components/ui/button'
	import { goto } from '$app/navigation'
	import { resolve } from '$app/paths'

	let { data }: PageProps = $props()
</script>

<div class="px-4 sm:px-0">
	<h1 class="pb-2 text-xl font-bold sm:text-2xl">{data.forum.title}</h1>

	<div class="grid grid-cols-3 items-center">
		<div></div>
		<Paginator
			currentPage={data.currentPage}
			perPage={data.perPage}
			totalCount={data.categoryCount}
		/>
		<div class="grid grid-flow-col justify-end gap-x-2">
			{#if data.canCreateCategory}
				<Button
					class={buttonVariants({ class: 'h-8' })}
					onclick={async () => {
						const path = resolve('/(app)/categories/create')
						const url = `${path}?forumId=${data.forum.forumId}`
						await goto(url)
					}}
				>
					<IconCategoryPlus class="size-4" />
					<ButtonTitle>Create category</ButtonTitle>
				</Button>
			{/if}
		</div>
	</div>
</div>

{#if data.forumData != null}
	<div class="mt-4 rounded-lg border px-4 py-2">
		{#each data.forumData.forumCategories as category, index (category.categoryId)}
			{@const latestPost = data.forumData.categoryLatestPosts.get(category.categoryId)}
			<CategoryView
				{category}
				threadCount={data.forumData.categoryThreadsCount.get(category.categoryId)}
				postCount={data.forumData.categoryPostsCount.get(category.categoryId)}
				{latestPost}
				users={data.forumData.users}
			/>
			{#if index < (data.forumData.forumCategories.length ?? 0) - 1}
				<Separator class="my-2" />
			{/if}
		{/each}
	</div>
{/if}
