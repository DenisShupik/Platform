<script lang="ts">
	import { Paginator } from '$lib/components/app'
	import { formatTimestamp } from '$lib/utils/formatTimestamp'
	import IconClockFilled from '~icons/tabler/clock-filled'
	import type { PageProps } from './$types'
	import { resolve } from '$app/paths'

	let { data }: PageProps = $props()
</script>

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
			{#each data.extraData.threadDrafts as thread (thread.threadId)}
				<tr class="border">
					<td class="border border-x-0 pl-2">
						<a
							href={resolve('/(app)/threads/[threadId=ThreadId]', {
								threadId: thread.threadId
							})}
							class="leading-none font-semibold tracking-tight"
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
