<script lang="ts">
	import { ForumView, Paginator } from '$lib/components/app'
	import type { PageProps } from './$types'
	let { data }: PageProps = $props()
</script>

<main class="flex flex-col items-center justify-center gap-y-4 py-8 sm:container">
	<Paginator currentPage={data.currentPage} perPage={data.perPage} totalCount={data.forumsCount} />
	<div class="mt-2 w-full space-y-4">
		{#each data.forums as forum}
			<ForumView
				{forum}
				categoryCount={data.forumCategoriesCount.get(forum.forumId) ?? 0n}
				categories={data.forumsCategoriesLatestByPost.get(forum.forumId) ?? []}
				categoryThreadsCount={data.categoryThreadsCount}
				categoryPostsCount={data.categoriesPostsCount}
				categoriesPostsLatest={data.categoriesPostsLatest}
				users={data.users}
			/>
		{/each}
	</div>
</main>
