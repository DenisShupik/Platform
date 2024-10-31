<script lang="ts">
  import { GET } from '$lib/GET'
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
  import BreadcrumbRouteLink from '$lib/components/ui/route-link/BreadcrumbRouteLink.svelte'
  import { categoryStore } from '$lib/stores/categoryStore'
  import { sectionStore } from '$lib/stores/sectionStore'

  let { categoryId }: { categoryId: Category['categoryId'] } = $props()

  let topicCount: number | undefined = $state()
  let topics: KeysetPage<Topic> | undefined = $state()
  let category = $derived($categoryStore.get(categoryId))
  let section = $derived(
    category === undefined ? undefined : $sectionStore.get(category.sectionId)
  )

  $effect(() => {
    if (category === undefined) {
      GET<Category>(`/categories/${categoryId}`).then((v) =>
        categoryStore.update((e) => {
          e.set(categoryId, v)
          return e
        })
      )
    }
  })

  $effect(() => {
    if (category !== undefined && section === undefined) {
      GET<Section>(`/sections/${category.sectionId}`).then((v) =>
        sectionStore.update((e) => {
          e.set(category.sectionId, v)
          return e
        })
      )
    }
  })

  $effect(() => {
    if (category !== undefined && topicCount === undefined) {
      GET<number>(`/categories/${categoryId}/topics/count`).then(
        (v) => (topicCount = v)
      )
    }
  })

  $effect(() => {
    if (category !== undefined && topics === undefined) {
      GET<KeysetPage<Topic>>(`/categories/${categoryId}/topics`).then(
        (v) => (topics = v)
      )
    }
  })

  const isDesktop = new MediaQuery('(min-width: 768px)')

  const perPage = $derived(isDesktop.matches ? 3 : 8)
  const siblingCount = $derived(isDesktop.matches ? 1 : 0)
</script>

{#if category === undefined}
  <p>Загрузка тем...</p>
{:else}
  <Breadcrumb.Root>
    <Breadcrumb.List>
      <Breadcrumb.Item>
        <BreadcrumbRouteLink link="/" title="Разделы" />
      </Breadcrumb.Item>
      <Breadcrumb.Separator />
      <Breadcrumb.Item>
        {#if section}
          <BreadcrumbRouteLink
            link={`/sections/${section.sectionId}`}
            title={section.title}
          />
        {:else}
          <div>Получаю данные</div>
        {/if}
      </Breadcrumb.Item>
    </Breadcrumb.List>
  </Breadcrumb.Root>
  <h1 class="text-2xl font-bold">{category?.title}</h1>
  {#if topicCount !== undefined}
    <Pagination.Root count={topicCount} {perPage} {siblingCount}>
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
  {#if topics != null}
    <table class="w-full table-fixed mt-4 border-collapse border">
      <colgroup>
        <col class="w-16" />
        <col />
        <col class="w-24 hidden md:table-column" />
        <col class="w-48 hidden md:table-column" />
        <col class="w-12 hidden md:table-column" />
      </colgroup>
      {#each topics.items as topic}
        <TopicView {topic} />
      {/each}
    </table>
  {/if}
{/if}
