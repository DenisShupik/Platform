<script lang="ts">
	import * as Collapsible from '$lib/components/ui/collapsible'
	import { buttonVariants } from '$lib/components/ui/button'
	import { Separator } from '$lib/components/ui/separator'
	// import CategoryView from './CategoryView.svelte'
	// import CreateCategoryDialog from './dialogs/CreateCategoryDialog.svelte'
	import { IconChevronUp } from '@tabler/icons-svelte'
	import { getForumsCategoriesLatestByPost, type Category, type Forum } from '$lib/utils/client'
	//import RouteLink from './ui/route-link/RouteLink.svelte'
	import { pluralize } from '$lib/utils/pluralize'
	// import {
	//   forumCategoriesCountLoader,
	//   forumCategoriesCountState
	// } from '$lib/states/forumCategoriesCountState.svelte'

	const forms: [string, string] = ['category', 'categories']

	let { forum }: { forum: Forum } = $props()

	let isOpen = $state(true)
</script>

<Collapsible.Root
	class="bg-card text-card-foreground grid w-full rounded-lg border shadow-sm"
	bind:open={isOpen}
>
	<div class="bg-muted/40 flex h-10 items-center px-4">
		<a href="/forums/{forum.forumId}" class="text-base font-semibold">{forum.title}</a>
		<div class="ml-auto flex items-center">
			<!-- {#if categoryCount === undefined}
				<Skeleton class="h-5 w-28" />
			{:else}
				<span class="w-28 whitespace-nowrap text-center text-sm font-light"
					>{categoryCount} {pluralize(categoryCount, forms)}</span
				>
			{/if}

			<CreateCategoryDialog
				forumId={forum.forumId}
				class={buttonVariants({ variant: 'ghost', class: 'h-8 gap-1' })}
			/> -->
			<Collapsible.Trigger
				class={buttonVariants({
					variant: 'ghost',
					class: 'size-8 p-2'
				})}
			>
				<IconChevronUp
					class={`transition-transform duration-200 ${isOpen ? 'rotate-180' : 'rotate-0'}`}
				/>
				<span class="sr-only">Toggle</span>
			</Collapsible.Trigger>
		</div>
	</div>
	<!-- {#if categories != null && categories.length !== 0}
		<Collapsible.Content class="px-4 py-2">
			{#each categories ?? [] as category, index}
				<CategoryView {category} />
				{#if index < (categories?.length ?? 0) - 1}
					<Separator class="my-2" />
				{/if}
			{/each}
		</Collapsible.Content>
	{/if} -->
</Collapsible.Root>
