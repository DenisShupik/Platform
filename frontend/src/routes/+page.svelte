<script lang="ts">
	import { ForumView, Paginator } from '$lib/components/app'
	import type { PageProps } from './$types'

	let { data }: PageProps = $props()
</script>

<main class="flex flex-col items-center justify-center gap-y-4 py-8 sm:container">
	<div class="mt-2 flex w-full items-center px-4 sm:px-0">
		{#if data.forumsData}
			<Paginator
				currentPage={data.currentPage}
				perPage={data.perPage}
				totalCount={data.forumsCount}
			/>
		{/if}
	</div>
	{#if data.forumsData}
		<div class="mt-2 w-full space-y-4">
			{#each data.forumsData.forums as forum}
				<ForumView
					{forum}
					categoryCount={data.forumsData.forumCategoriesCount.get(forum.forumId) ?? 0n}
					categories={data.forumsData.forumsCategoriesLatest.get(forum.forumId) ?? []}
					categoryThreadsCount={data.forumsData.categoriesThreadsCount}
					categoryPostsCount={data.forumsData.categoriesPostsCount}
					categoriesPostsLatest={data.forumsData.categoriesPostsLatest}
					users={data.forumsData.users}
				/>
			{/each}
		</div>
	{/if}
</main>
