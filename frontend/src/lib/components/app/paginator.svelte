<script lang="ts">
	import { goto } from '$app/navigation'
	import * as Pagination from '$lib/components/ui/pagination'
	import { IconChevronLeft, IconChevronRight } from '@tabler/icons-svelte'
	import { MediaQuery } from 'svelte/reactivity'
	import { page } from '$app/state'

	let {
		pageIndex,
		perPage,
		count
	}: {
		pageIndex: bigint
		perPage: bigint
		count: bigint
	} = $props()

	const isDesktop = new MediaQuery('(min-width: 768px)')
	const siblingCount = $derived(isDesktop.current ? 1 : 0)
</script>

{#if count !== undefined}
	<Pagination.Root
		count={Number(count)}
		perPage={Number(perPage)}
		{siblingCount}
		page={Number(pageIndex)}
		onPageChange={(p) => {
			const url = new URL(page.url)
			url.searchParams.set('page', p)
			goto(url.pathname + url.search + url.hash)
		}}
		class="mt-2"
	>
		{#snippet children({ pages, currentPage })}
			<Pagination.Content>
				<Pagination.Item>
					<Pagination.PrevButton>
						<IconChevronLeft class="size-4" />
						<span class="hidden sm:block">Previous</span>
					</Pagination.PrevButton>
				</Pagination.Item>
				{#each pages as page (page.key)}
					{#if page.type === 'ellipsis'}
						<Pagination.Item>
							<Pagination.Ellipsis />
						</Pagination.Item>
					{:else}
						<Pagination.Item>
							<Pagination.Link {page} isActive={currentPage === page.value}>
								{page.value}
							</Pagination.Link>
						</Pagination.Item>
					{/if}
				{/each}
				<Pagination.Item>
					<Pagination.NextButton>
						<span class="hidden sm:block">Next</span>
						<IconChevronRight class="size-4" />
					</Pagination.NextButton>
				</Pagination.Item>
			</Pagination.Content>
		{/snippet}
	</Pagination.Root>
{/if}
