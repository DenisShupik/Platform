<script lang="ts">
	import { goto } from '$app/navigation'
	import { ForumView, Paginator } from '$lib/components/app'
	import { Button } from '$lib/components/ui/button'
	import * as Select from '$lib/components/ui/select'
	import { IconFilterCog } from '@tabler/icons-svelte'
	import type { PageProps } from './$types'
	import * as Popover from '$lib/components/ui/popover'
	import Label from '$lib/components/ui/label/label.svelte'

	let { data }: PageProps = $props()

	const forumContainsOptions = [
		{ value: 'all', label: 'All' },
		{ value: 'categories', label: 'Only with categories' },
		{ value: 'threads', label: 'Only with threads' },
		{ value: '', label: 'Only with posts' }
	]

	let formContainsFilter = $state(data.contains)

	const triggerContent = $derived(
		forumContainsOptions.find((f) => f.value === formContainsFilter)?.label ?? ''
	)

	$effect(() => {
		const params = new URLSearchParams(window.location.search)
		switch (formContainsFilter) {
			case 'all':
			case 'categories':
			case 'threads':
				params.set('contains', formContainsFilter)
				break
			default:
				params.delete('contains')
				break
		}
		goto(`?${params.toString()}`, { replaceState: true })
	})
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
		<Popover.Root>
			<Popover.Trigger>
				<Button variant="outline" size="icon" class="[&_svg]:size-10 [&_svg]:p-2.5">
					<IconFilterCog />
				</Button>
			</Popover.Trigger>
			<Popover.Content class="mx-2 w-80">
				<div class="flex items-center space-x-2">
					<Label class="text-muted-foreground text-sm font-normal">Forum visibility</Label>
					<Select.Root type="single" name="forum-visibility" bind:value={formContainsFilter}>
						<Select.Trigger class="w-full">
							{triggerContent}
						</Select.Trigger>
						<Select.Content>
							<Select.Group>
								{#each forumContainsOptions as option (option.value)}
									<Select.Item value={option.value} label={option.label}>
										{option.label}
									</Select.Item>
								{/each}
							</Select.Group>
						</Select.Content>
					</Select.Root>
				</div>
			</Popover.Content>
		</Popover.Root>
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
