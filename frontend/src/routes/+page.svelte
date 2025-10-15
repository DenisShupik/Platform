<script lang="ts">
	import { ForumView, Paginator } from '$lib/components/app'
	import type { PageProps } from './$types'
	import { Button, buttonVariants } from '$lib/components/ui/button'
	import { goto } from '$app/navigation'
	import { resolve } from '$app/paths'
	import { IconTextPlus } from '@tabler/icons-svelte'
	import { PolicyType } from '$lib/utils/client'

	let { data }: PageProps = $props()
</script>

<main class="py-8 sm:container">
	<div class="flex w-full items-center justify-between gap-x-2 px-4 sm:px-0">
		<h1 class="flex-1 text-xl font-bold sm:text-2xl">Forums</h1>
		{#if data.permissions[PolicyType.FORUM_CREATE]}
			<Button
				class={buttonVariants({ class: 'h-8' })}
				onclick={async () => {
					await goto(resolve('/(app)/forums/create'))
				}}
			>
				<IconTextPlus class="size-4" />Create forum</Button
			>
		{/if}
	</div>

	{#if data.forumsData}
		<Paginator
			currentPage={data.currentPage}
			perPage={data.perPage}
			totalCount={data.forumsCount}
		/>
	{/if}

	{#if data.forumsData}
		<div class="mt-4 w-full space-y-4">
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
