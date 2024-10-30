<script lang="ts">
  import { get } from '$lib/get'
  import type { KeysetPage } from '$lib/types/KeysetPage'
  import type { Topic } from '$lib/types/Topic'
  import TopicView from '$lib/components/TopicView.svelte'
  import * as Breadcrumb from '$lib/components/ui/breadcrumb'
  import type { Category } from '$lib/types/Category'
  import type { Section } from '$lib/types/Section'
  import * as Pagination from '$lib/components/ui/pagination'
  import ChevronLeft from 'lucide-svelte/icons/chevron-left'
  import ChevronRight from 'lucide-svelte/icons/chevron-right'
  import { MediaQuery } from 'runed'
  import BreadcrumbRouteLink from '../ui/route-link/BreadcrumbRouteLink.svelte'

  let { categoryId }: { categoryId: Pick<Category, 'categoryId'> } = $props()

  async function init() {
    const category = await get<Category>(`/categories/${categoryId}`)

    const [topicsCount, topics, section] = await Promise.all([
      get<number>(`/categories/${categoryId}/topics/count`),
      get<KeysetPage<Topic>>(`/categories/${categoryId}/topics`),
      get<Section>(`/sections/${category.sectionId}`)
    ])
    return { category, topicsCount, topics, section }
  }

  let promise: Promise<{
    category: Category
    topicsCount: number
    topics: KeysetPage<Topic>
    section: Section
  }> = $state(init())

  const isDesktop = new MediaQuery('(min-width: 768px)')

  const perPage = $derived(isDesktop.matches ? 3 : 8)
  const siblingCount = $derived(isDesktop.matches ? 1 : 0)
</script>

{#await promise}
  <p>Загрузка тем...</p>
{:then { category, topicsCount, topics, section }}
  <Breadcrumb.Root>
    <Breadcrumb.List>
      <Breadcrumb.Item>
        <BreadcrumbRouteLink link="/" title="Разделы" />
      </Breadcrumb.Item>
      <Breadcrumb.Separator />
      <Breadcrumb.Item>
        <BreadcrumbRouteLink
          link={`/sections/${section.sectionId}`}
          title={section.title}
        />
      </Breadcrumb.Item>
    </Breadcrumb.List>
  </Breadcrumb.Root>
  <h1 class="text-2xl font-bold">{category.title}</h1>
  <Pagination.Root count={topicsCount} {perPage} {siblingCount}>
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
  <div class="space-y-4 pt-8">
    {#each topics.items as topic}
      <TopicView {topic} />
    {/each}
  </div>
{:catch error}
  <p style="color: red;">{error}</p>
{/await}
