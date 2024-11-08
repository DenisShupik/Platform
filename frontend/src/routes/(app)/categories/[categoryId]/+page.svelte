<script lang="ts">
  import TopicView from '$lib/components/ThreadView.svelte'
  import * as Breadcrumb from '$lib/components/ui/breadcrumb'
  import BreadcrumbRouteLink from '$lib/components/ui/route-link/BreadcrumbRouteLink.svelte'
  import { categoryStore } from '$lib/stores/categoryStore'
  import { forumStore } from '$lib/stores/forumStore'
  import Paginator from '$lib/components/Paginator.svelte'
  import { page } from '$app/stores'
  import {
    getCategory,
    getCategoryThreads,
    getCategoryThreadsCount,
    getForum,
    type Category,
    type Thread
  } from '$lib/utils/client'

  let categoryId: Category['categoryId'] = $derived(
    parseInt($page.params.categoryId)
  )
  let perPage = $state(5)
  let currentPage: number = $state(1)

  let threadCount: number | undefined = $state()
  let topics: Thread[] | undefined = $state()
  let category = $derived($categoryStore.get(categoryId))
  let forum = $derived(
    category === undefined ? undefined : $forumStore.get(category.forumId)
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
      getCategoryThreads({
        path: { categoryId },
        query: { cursor: (currentPage - 1) * perPage, limit: perPage },
        signal
      })
        .then((v) => (topics = v.data?.items))
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
      getCategory<true>({ path: { categoryId } }).then((v) =>
        categoryStore.update((e) => {
          e.set(categoryId, v.data)
          return e
        })
      )
    }
  })

  $effect(() => {
    if (category != null && forum === undefined) {
      const forumId = category.forumId
      getForum<true>({ path: { forumId } }).then((v) =>
        forumStore.update((e) => {
          e.set(forumId, v.data)
          return e
        })
      )
    }
  })

  $effect(() => {
    if (category != null && threadCount === undefined) {
      const categoryId = category.categoryId
      getCategoryThreadsCount({ path: { categoryId } }).then(
        (v) => (threadCount = v.data)
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
        {#if forum}
          <BreadcrumbRouteLink
            link={`/forums/${forum.forumId}`}
            title={forum.title}
          />
        {:else}
          <div>Получаю данные</div>
        {/if}
      </Breadcrumb.Item>
    </Breadcrumb.List>
  </Breadcrumb.Root>
  <h1 class="text-2xl font-bold">{category?.title}</h1>
  <Paginator bind:page={currentPage} {perPage} count={threadCount} />
  {#if topics != null}
    <table class="mt-4 w-full table-fixed border-collapse border">
      <colgroup>
        <col class="w-16" />
        <col />
        <col class="hidden w-24 md:table-column" />
        <col class="hidden w-32 md:table-column" />
        <col class="hidden w-12 md:table-column" />
      </colgroup>
      {#each topics as topic}
        <TopicView {topic} />
      {/each}
    </table>
  {/if}
{/if}
