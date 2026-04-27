<script lang="ts">
	import { ButtonTitle, ForumBreadcrumb, Paginator, ThreadView } from '$lib/components/app'
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import { Button, buttonVariants } from '$lib/components/ui/button'
	import IconTextPlus from '~icons/tabler/text-plus'
	import type { PageProps } from './$types'
	import { goto } from '$app/navigation'
	import { resolve } from '$app/paths'

	let { data }: PageProps = $props()
</script>

<div class="px-4 sm:px-0">
	<Breadcrumb.Root>
		<Breadcrumb.List>
			<ForumBreadcrumb forum={data.forum} />
		</Breadcrumb.List>
	</Breadcrumb.Root>

	<h1 class="pb-2 text-xl font-bold sm:text-2xl">{data.category.title}</h1>

	<div class="grid grid-cols-3 items-center">
		<div></div>
		<Paginator
			currentPage={data.currentPage}
			perPage={data.perPage}
			totalCount={data.categoryThreadsCount}
		/>
		<div class="grid grid-flow-col justify-end gap-x-2">
			{#if data.canCreateThread}
				<Button
					class={buttonVariants({ class: 'h-8' })}
					onclick={async () => {
						const path = resolve('/(app)/threads/create')
						const url = `${path}?categoryId=${data.category.categoryId}`
						await goto(url)
					}}
				>
					<IconTextPlus class="size-4" />
					<ButtonTitle>Create thread</ButtonTitle>
				</Button>
			{/if}
		</div>
	</div>
</div>

{#if data.categoryData}
	<table class="mt-4 w-full table-auto border-collapse border">
		<colgroup>
			<col class="w-20" />
			<col />
			<col class="hidden w-24 md:table-column" />
			<col class="hidden w-52 md:table-column" />
		</colgroup>
		<tbody>
			{#each data.categoryData.categoryThreads as thread (thread.threadId)}
				<ThreadView
					{thread}
					postCount={data.categoryData.threadsPostsCount.get(thread.threadId) ?? 0}
					latestPost={data.categoryData.threadsPostsLatest.get(thread.threadId)}
					users={data.categoryData.users}
				/>
			{/each}
		</tbody>
	</table>
{/if}
