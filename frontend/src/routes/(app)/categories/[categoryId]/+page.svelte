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
    getCategoryThreadsCount,
    getForum,
    type Category,
    type Thread
  } from '$lib/utils/client'
  import { categoryThreadsCountLoader } from '$lib/dataLoaders/categoryThreadsCountLoader'

  let categoryId: Category['categoryId'] = $derived(
    parseInt($page.params.categoryId)
  )
  let perPage = $state(5)
  let currentPage: number = $state(1)

  let threadCount: number | undefined = $state()
  let threads: Thread[] | undefined = $state()
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
        .then((v) => (threads = v.data?.items))
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
      categoryThreadsCountLoader
        .load(category.categoryId)
        .then((v) => (threadCount = v))
    }
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
  <Paginator bind:page={currentPage} {perPage} count={threadCount} />
  {#if threads != null}
    <table class="mt-4 w-full table-auto border-collapse border">
      <colgroup>
        <col class="w-20" />
        <col />
        <col class="hidden w-24 md:table-column" />
        <col class="hidden w-52 md:table-column" />
      </colgroup>
      {#each threads as thread}
        <ThreadView {thread} />
      {/each}
    </table>
  {/if}
{/if}
