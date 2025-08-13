<script lang="ts">
	import { Paginator } from '$lib/components/app'
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import { formatTimestamp } from '$lib/utils/formatTimestamp'
	import { IconClockFilled } from '@tabler/icons-svelte'
	import type { PageProps } from './$types'
	import { resolve } from '$app/paths'

	let { data }: PageProps = $props()
</script>

<Breadcrumb.Root>
	<Breadcrumb.List class="px-4 sm:px-0">
		<Breadcrumb.Item>
			<a href={resolve('/')}>Forums</a>
		</Breadcrumb.Item>
	</Breadcrumb.List>
</Breadcrumb.Root>

{#if data.extraData}
	<Paginator
		currentPage={data.currentPage}
		perPage={data.perPage}
		totalCount={data.threadDraftsCount}
	/>
	<table class="mt-4 w-full table-auto border-collapse border">
		<colgroup>
			<col />
		</colgroup>
		<tbody>
			{#each data.extraData.threadDrafts as thread}
				<tr class="border">
					<td class="border border-x-0 pl-2">
						<a
							href={resolve('/(app)/threads/[threadId=ThreadId]/draft', { threadId: thread.threadId })}
							class="font-semibold leading-none tracking-tight"
							>{thread.title}
						</a>
						<p class="text-muted-foreground flex items-center gap-x-1 text-sm">
							<IconClockFilled class="inline size-3" /><time
								>{formatTimestamp(thread.createdAt)}</time
							>
						</p>
					</td>
				</tr>
			{/each}
		</tbody>
	</table>
{/if}
