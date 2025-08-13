<script lang="ts">
	import * as Collapsible from '$lib/components/ui/collapsible'
	import { buttonVariants } from '$lib/components/ui/button'
	import { Separator } from '$lib/components/ui/separator'
	import { CategoryView } from '$lib/components/app'
	import { IconChevronUp } from '@tabler/icons-svelte'
	import {
		type CategoryDto,
		type CategoryId,
		type ForumDto,
		type PostDto,
		type UserDto,
		type UserId
	} from '$lib/utils/client'
	import { pluralize } from '$lib/utils/pluralize'
	import { resolve } from '$app/paths'

	const forms: [string, string] = ['category', 'categories']

	let {
		forum,
		categoryCount,
		categories,
		categoryThreadsCount,
		categoryPostsCount,
		categoriesPostsLatest,
		users
	}: {
		forum: ForumDto
		categoryCount: bigint
		categories: CategoryDto[]
		categoryThreadsCount: Map<CategoryId, bigint>
		categoryPostsCount: Map<CategoryId, bigint>
		categoriesPostsLatest: Map<CategoryId, PostDto>
		users: Map<UserId, UserDto>
	} = $props()

	let isOpen = $state(true)
</script>

<Collapsible.Root
	class="bg-card text-card-foreground grid w-full rounded-lg border shadow-sm"
	bind:open={isOpen}
>
	<div class="bg-muted/40 flex h-10 items-center px-4">
		<a
			href={resolve('/(app)/forums/[forumId=ForumId]', { forumId: forum.forumId })}
			class="text-base font-semibold">{forum.title}</a
		>
		<div class="ml-auto flex items-center">
			<span class="w-28 whitespace-nowrap text-center text-sm font-light"
				>{categoryCount} {pluralize(categoryCount, forms)}</span
			>
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
	{#if categories != null && categories.length !== 0}
		<Collapsible.Content class="px-4 py-2">
			{#each categories ?? [] as category, index}
				{@const latestPost = categoriesPostsLatest.get(category.categoryId)}
				<CategoryView
					{category}
					threadCount={categoryThreadsCount.get(category.categoryId) ?? 0n}
					postCount={categoryPostsCount.get(category.categoryId) ?? 0n}
					{latestPost}
					{users}
				/>
				{#if index < (categories?.length ?? 0) - 1}
					<Separator class="my-2" />
				{/if}
			{/each}
		</Collapsible.Content>
	{/if}
</Collapsible.Root>
