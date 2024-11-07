<script lang="ts">
  import { GET } from '$lib/utils/GET'
  import type { KeysetPage } from '$lib/types/KeysetPage'
  import type { Topic } from '$lib/types/Topic'
  import TopicView from '$lib/components/TopicView.svelte'
  import * as Breadcrumb from '$lib/components/ui/breadcrumb'
  import type { Category } from '$lib/types/Category'
  import type { Section } from '$lib/types/Section'
  import BreadcrumbRouteLink from '$lib/components/ui/route-link/BreadcrumbRouteLink.svelte'
  import { categoryStore } from '$lib/stores/categoryStore'
  import { sectionStore } from '$lib/stores/sectionStore'
  import Paginator from '$lib/components/Paginator.svelte'
  import { page } from '$app/stores'

  let categoryId: Category['categoryId'] = $derived(
    parseInt($page.params.categoryId)
  )
  let perPage = $state(5)
  let currentPage: number = $state(1)

  let topicCount: number | undefined = $state()
  let topics: KeysetPage<Topic> | undefined = $state()
  let category = $derived($categoryStore.get(categoryId))
  let section = $derived(
    category === undefined ? undefined : $sectionStore.get(category.sectionId)
  )

  let fetchAbortController: AbortController | null = null

  $effect(() => {
    if (category !== undefined) {
      if (fetchAbortController) {
        fetchAbortController.abort()
      }
      const abortController = new AbortController()
      const signal = abortController.signal
      fetchAbortController = abortController
      GET<KeysetPage<Topic>>(
        `/categories/${category.categoryId}/topics?cursor=${(currentPage - 1) * perPage}&limit=${perPage}`,
        { signal }
      )
        .then((v) => (topics = v))
        .catch((error) => {
          if (error.name !== 'AbortError') throw error
        })
        .finally(() => {
          if (fetchAbortController === abortController)
            fetchAbortController = null
        })
    }
  })

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
  <Paginator bind:page={currentPage} {perPage} count={topicCount} />
  {#if topics != null}
    <table class="mt-4 w-full table-fixed border-collapse border">
      <colgroup>
        <col class="w-16" />
        <col />
        <col class="hidden w-24 md:table-column" />
        <col class="hidden w-32 md:table-column" />
        <col class="hidden w-12 md:table-column" />
      </colgroup>
      {#each topics.items as topic}
        <TopicView {topic} />
      {/each}
    </table>
  {/if}
{/if}
