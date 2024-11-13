<script lang="ts">
  import { goto } from '$app/navigation'
  import * as Pagination from '$lib/components/ui/pagination'
  import ChevronLeft from '@tabler/icons-svelte/icons/chevron-left'
  import ChevronRight from '@tabler/icons-svelte/icons/chevron-right'
  import { MediaQuery } from 'runed'
  import { page } from '$app/stores'
  import { getPageFromUrl } from '$lib/utils/tryParseInt'

  let {
    perPage,
    count
  }: {
    perPage: number
    count: number | undefined
  } = $props()

  const isDesktop = new MediaQuery('(min-width: 768px)')
  const siblingCount = $derived(isDesktop.matches ? 1 : 0)

  let currentPage: number = $derived(getPageFromUrl($page.url))
</script>

{#if count !== undefined}
  <Pagination.Root
    {count}
    {perPage}
    {siblingCount}
    controlledPage
    page={currentPage}
    onPageChange={(p) => {
      const url = new URL($page.url)
      url.searchParams.set('page', p)
      goto(url.pathname + url.search + url.hash)
    }}
    class="mt-2"
  >
    {#snippet children({ pages, currentPage })}
      <Pagination.Content>
        <Pagination.Item>
          <Pagination.PrevButton>
            <ChevronLeft class="size-4" />
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
            <ChevronRight class="size-4" />
          </Pagination.NextButton>
        </Pagination.Item>
      </Pagination.Content>
    {/snippet}
  </Pagination.Root>
{/if}
