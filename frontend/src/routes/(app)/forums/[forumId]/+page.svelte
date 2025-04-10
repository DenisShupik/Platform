<script lang="ts">
	import { CategoryView, Paginator } from '$lib/components/app'
	import { Separator } from '$lib/components/ui/separator'
	import type { PageProps } from './$types'

	let { data }: PageProps = $props()
</script>

<Paginator currentPage={data.currentPage} perPage={data.perPage} totalCount={data.categoryCount} />

<h1 class="text-2xl font-bold">{data.forum.title}</h1>
{#if data.forumCategories != null}
	<div class="mt-4 rounded-lg border px-4 py-2">
		{#each data.forumCategories ?? [] as category, index}
			{@const latestPost = data.categoryLatestPosts.get(category.categoryId)}
			<CategoryView
				{category}
				threadCount={data.categoryThreadsCount.get(category.categoryId) ?? 0n}
				postCount={data.categoryPostsCount.get(category.categoryId) ?? 0n}
				{latestPost}
				author={data.users.get(latestPost.createdBy)}
			/>
			{#if index < (data.forumCategories.length ?? 0) - 1}
				<Separator class="my-2" />
			{/if}
		{/each}
	</div>
{/if}
