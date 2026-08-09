<script lang="ts">
	import { AppContainer, ButtonTitle, ForumView, Paginator } from '$lib/components/app'
	import type { PageProps } from './$types'
	import { Button } from '$lib/components/ui/button'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import IconTextPlus from '~icons/tabler/text-plus'
	import * as m from '$lib/paraglide/messages'
	import { resolve } from '$app/paths'

	let { data }: PageProps = $props()
</script>

<svelte:head>
	<title>{m.forums()} — {PUBLIC_APP_NAME}</title>
</svelte:head>

<main id="main-content" tabindex="-1" class="py-8">
	<AppContainer>
		<div>
			<h1 class="pb-2 text-xl font-bold sm:text-2xl">{m.forums()}</h1>

			<div class="grid grid-cols-3 items-center">
				<div></div>
				<Paginator
					currentPage={data.currentPage}
					perPage={data.perPage}
					totalCount={data.forumsCount}
				/>
				<div class="grid justify-end gap-x-2">
					{#if data.canCreateForum}
						<Button href={resolve('/(app)/forums/create')} class="h-8">
							<IconTextPlus class="size-4" />
							<ButtonTitle>{m.forum_create()}</ButtonTitle>
						</Button>
					{/if}
				</div>
			</div>
		</div>

		{#if data.forumsData}
			<div class="mt-4 flex w-full flex-col gap-4">
				{#each data.forumsData.forums as forum (forum.forumId)}
					<ForumView
						{forum}
						categoryCount={data.forumsData.forumCategoriesCount.get(forum.forumId)}
						categories={data.forumsData.forumsCategoriesLatest.get(forum.forumId) ?? []}
						categoryThreadsCount={data.forumsData.categoriesThreadsCount}
						categoryPostsCount={data.forumsData.categoriesPostsCount}
						categoriesPostsLatest={data.forumsData.categoriesPostsLatest}
						users={data.forumsData.users}
					/>
				{/each}
			</div>
		{/if}
	</AppContainer>
</main>
