<script lang="ts">
	import { goto } from '$app/navigation'
	import { page } from '$app/state'
	import * as Pagination from '$lib/components/ui/pagination'
	import IconChevronLeft from '~icons/tabler/chevron-left'
	import IconChevronRight from '~icons/tabler/chevron-right'
	import type { ClassValue } from 'svelte/elements'
	import { MediaQuery } from 'svelte/reactivity'
	import type { Count } from '$lib/utils/client'

	let {
		class: className,
		currentPage,
		perPage,
		totalCount
	}: { class?: ClassValue; currentPage: number; perPage: number; totalCount: Count } = $props()

	const isDesktop = new MediaQuery('(min-width: 768px)')
	const siblingCount = $derived(isDesktop.current ? 1 : 0)
</script>

<Pagination.Root
	class={className}
	count={totalCount}
	{perPage}
	page={currentPage}
	{siblingCount}
	onPageChange={(p) => {
		const url = new URL(page.url)
		url.searchParams.set('page', p)
		goto(url.pathname + url.search + url.hash)
	}}
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
