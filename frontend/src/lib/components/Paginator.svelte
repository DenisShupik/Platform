<script lang="ts">
  import * as Pagination from '$lib/components/ui/pagination'
  import ChevronLeft from '@tabler/icons-svelte/icons/chevron-left'
  import ChevronRight from '@tabler/icons-svelte/icons/chevron-right'
  import { MediaQuery } from 'runed'

  let { count }: { count: number | undefined } = $props()

  const isDesktop = new MediaQuery('(min-width: 768px)')

  const perPage = $derived(isDesktop.matches ? 3 : 8)
  const siblingCount = $derived(isDesktop.matches ? 1 : 0)
</script>

{#if count !== undefined}
  <Pagination.Root {count} {perPage} {siblingCount}>
    {#snippet children({ pages, currentPage })}
      <Pagination.Content>
        <Pagination.Item>
          <Pagination.PrevButton>
            <ChevronLeft class="size-4" />
            <span class="hidden sm:block">Назад</span>
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
            <span class="hidden sm:block">Вперед</span>
            <ChevronRight class="size-4" />
          </Pagination.NextButton>
        </Pagination.Item>
      </Pagination.Content>
    {/snippet}
  </Pagination.Root>
{/if}
