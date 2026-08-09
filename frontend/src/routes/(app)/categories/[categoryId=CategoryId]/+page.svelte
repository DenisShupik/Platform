<script lang="ts">
	import { ButtonTitle, ForumBreadcrumb, Paginator, ThreadView } from '$lib/components/app'
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import { Button } from '$lib/components/ui/button'
	import IconTextPlus from '~icons/tabler/text-plus'
	import type { PageProps } from './$types'
	import { resolve } from '$app/paths'
	import { zeroCount } from '$lib/utils/value-object'
	import { PUBLIC_APP_NAME } from '$env/static/public'

	let { data }: PageProps = $props()

	const createThreadHref = $derived(
		`${resolve('/(app)/threads/create')}?${new URLSearchParams({ categoryId: data.category.categoryId })}`
	)
</script>

<svelte:head>
	<title>{data.category.title} — {data.forum.title} — {PUBLIC_APP_NAME}</title>
</svelte:head>

<div>
	<Breadcrumb.Root>
		<Breadcrumb.List>
			<ForumBreadcrumb forum={data.forum} />
			<Breadcrumb.Separator />
			<Breadcrumb.Item>
				<Breadcrumb.Page>{data.category.title}</Breadcrumb.Page>
			</Breadcrumb.Item>
		</Breadcrumb.List>
	</Breadcrumb.Root>

	<h1 class="mt-3 pb-2 text-xl font-bold sm:text-2xl">{data.category.title}</h1>

	<div class="grid grid-cols-3 items-center">
		<div></div>
		<Paginator
			currentPage={data.currentPage}
			perPage={data.perPage}
			totalCount={data.categoryThreadsCount}
		/>
		<div class="grid grid-flow-col justify-end gap-x-2">
			{#if data.canCreateThread}
				<Button href={createThreadHref} class="h-8">
					<IconTextPlus data-icon="inline-start" />
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
					postCount={data.categoryData.threadsPostsCount.get(thread.threadId) ?? zeroCount}
					latestPost={data.categoryData.threadsPostsLatest.get(thread.threadId)}
					users={data.categoryData.users}
				/>
			{/each}
		</tbody>
	</table>
{/if}
