<script lang="ts">
	import { ButtonTitle, CategoryView, Paginator } from '$lib/components/app'
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import { Separator } from '$lib/components/ui/separator'
	import IconCategoryPlus from '~icons/tabler/category-plus'
	import type { PageProps } from './$types'
	import { Button } from '$lib/components/ui/button'
	import { resolve } from '$app/paths'
	import { PUBLIC_APP_NAME } from '$env/static/public'

	let { data }: PageProps = $props()

	const createCategoryHref = $derived(
		`${resolve('/(app)/categories/create')}?${new URLSearchParams({ forumId: data.forum.forumId })}`
	)
</script>

<svelte:head>
	<title>{data.forum.title} — Forums — {PUBLIC_APP_NAME}</title>
</svelte:head>

<div>
	<Breadcrumb.Root>
		<Breadcrumb.List>
			<Breadcrumb.Item><a href={resolve('/')}>Forums</a></Breadcrumb.Item>
			<Breadcrumb.Separator />
			<Breadcrumb.Item><Breadcrumb.Page>{data.forum.title}</Breadcrumb.Page></Breadcrumb.Item>
		</Breadcrumb.List>
	</Breadcrumb.Root>

	<h1 class="mt-3 pb-2 text-xl font-bold sm:text-2xl">{data.forum.title}</h1>

	<div class="grid grid-cols-3 items-center">
		<div></div>
		<Paginator
			currentPage={data.currentPage}
			perPage={data.perPage}
			totalCount={data.categoryCount}
		/>
		<div class="grid grid-flow-col justify-end gap-x-2">
			{#if data.canCreateCategory}
				<Button href={createCategoryHref} class="h-8">
					<IconCategoryPlus data-icon="inline-start" />
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
