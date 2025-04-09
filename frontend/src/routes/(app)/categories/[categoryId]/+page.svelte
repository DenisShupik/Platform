<script lang="ts">
	import { Paginator, ThreadView } from '$lib/components/app'
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import type { PageProps } from './$types'

	let { data }: PageProps = $props()
</script>

<Breadcrumb.Root>
	<Breadcrumb.List>
		<Breadcrumb.Item>
			<a href="/">Forums</a>
		</Breadcrumb.Item>
		<Breadcrumb.Separator />
		<Breadcrumb.Item>
			<a href={`/forums/${data.forum.forumId}`}>{data.forum.title}</a>
		</Breadcrumb.Item>
	</Breadcrumb.List>
</Breadcrumb.Root>

<Paginator currentPage={data.currentPage} perPage={data.perPage} totalCount={data.categoryThreadsCount} />

<h1 class="text-2xl font-bold">{data.category.title}</h1>
{#if data.threads != null}
	<table class="mt-4 w-full table-auto border-collapse border">
		<colgroup>
			<col class="w-20" />
			<col />
			<col class="hidden w-24 md:table-column" />
			<col class="hidden w-52 md:table-column" />
		</colgroup>
		<tbody>
		{#each data.threads ?? [] as thread}
			<ThreadView
				{thread}
				postCount={data.threadPostsCount.get(thread.threadId) ?? 0n}
				latestPost={data.threadPostsLatest.get(thread.threadId)}
				users={data.users}
			/>
		{/each}
		</tbody>
	</table>
{/if}
