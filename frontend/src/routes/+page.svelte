<script lang="ts">
	import { ForumView } from '$lib/components/app'
	import type { PageProps } from './$types'
	let { data }: PageProps = $props()
</script>

<main class="flex max-w-3xl flex-col items-center justify-center gap-y-4 pt-8 sm:container">
	<div class="mt-2 space-y-4">
		{#each data.forums as forum}
			<ForumView
				{forum}
				categoryCount={data.forumCategoriesCount.get(forum.forumId) ?? 0n}
				categories={data.forumsCategoriesLatestByPost.get(forum.forumId) ?? []}
				categoryThreadsCount={data.categoryThreadsCount}
				categoryPostsCount={data.categoryPostsCount}
				categoryLatestPosts={data.categoryLatestPosts}
				users={data.users}
			/>
		{/each}
	</div>
</main>
