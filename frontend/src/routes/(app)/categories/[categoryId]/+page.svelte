<script lang="ts">
  import ThreadView from '$lib/components/ThreadView.svelte'
  import * as Breadcrumb from '$lib/components/ui/breadcrumb'
  import BreadcrumbRouteLink from '$lib/components/ui/route-link/BreadcrumbRouteLink.svelte'
  import { categoryStore } from '$lib/stores/categoryStore'
  import { forumStore } from '$lib/stores/forumStore'
  import Paginator from '$lib/components/Paginator.svelte'
  import { page } from '$app/stores'
  import {
    getCategory,
    getCategoryThreads,
    getForum,
    type Category,
    type Thread
  } from '$lib/utils/client'
  import type { FetchPageContext } from '$lib/types/fetchPageContext'
  import {
    categoryThreadsCountLoader,
    categoryThreadsCountState
  } from '$lib/stores/categoryThreadsCountState.svelte'

  let categoryId: Category['categoryId'] = $derived(
    parseInt($page.params.categoryId)
  )

  let perPage = $state(5)
  let currentPage: number = $state(1)

  let threadCount: number | undefined = $derived(
    categoryThreadsCountState.get(categoryId)
  )

  let pageState: {
    pages: (Thread[] | undefined)[]
  } = $state({ pages: [] })

  let category = $derived($categoryStore.get(categoryId))
  let forum = $derived(
    category === undefined ? undefined : $forumStore.get(category.forumId)
  )

  let fetchPageContext: FetchPageContext
  $effect(() => {
    const pageId = currentPage
    if (pageState.pages[pageId] === undefined) {
      if (fetchPageContext) {
        if (fetchPageContext.pageId === pageId) return
        fetchPageContext.abortController.abort()
      }
      const abortController = new AbortController()
      const signal = abortController.signal
      fetchPageContext = { abortController, pageId }
      getCategoryThreads({
        path: { categoryId },
        query: { cursor: (currentPage - 1) * perPage, limit: perPage },
        signal
      })
        .then((v) => {
          pageState.pages[pageId] = v.data?.items
        })
        .catch((error) => {
          if (error.name !== 'AbortError') throw error
        })
        .finally(() => {
          if (fetchPageContext?.abortController === abortController)
            fetchPageContext = undefined
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
    if (threadCount !== undefined) return
    categoryThreadsCountLoader
      .load(categoryId)
      .then((v) => categoryThreadsCountState.set(categoryId, v))
  })
</script>

{#if category === undefined}
  <p>Загрузка тем...</p>
{:else}
  <Breadcrumb.Root>
    <Breadcrumb.List>
      <Breadcrumb.Item>
        <BreadcrumbRouteLink link="/" title="Forums" />
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
  <Paginator {perPage} count={pageState.threadCount} />
  {#if pageState.pages[currentPage] != null}
    <table class="mt-4 w-full table-auto border-collapse border">
      <colgroup>
        <col class="w-20" />
        <col />
        <col class="hidden w-24 md:table-column" />
        <col class="hidden w-52 md:table-column" />
      </colgroup>
      {#each pageState.pages[currentPage] ?? [] as thread}
        <ThreadView {thread} />
      {/each}
    </table>
  {/if}
{/if}
