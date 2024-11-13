<script lang="ts" module>
  import { writable } from 'svelte/store'
  import { type Category } from '$lib/utils/client'
  interface Store {
    categoryCount?: number
    pages: (Category[] | undefined)[]
  }

  export let store = writable<Store>({ pages: [] })

  export const invalidate = () => {
    store.update((s) => (s = { pages: [] }))
  }
</script>

<script lang="ts">
  import { page } from '$app/stores'
  import CategoryView from '$lib/components/CategoryView.svelte'
  import Paginator from '$lib/components/Paginator.svelte'
  import { Separator } from '$lib/components/ui/separator'
  import { forumStore } from '$lib/stores/forumStore'
  import {
    getForum,
    getForumCategories,
    getForumCategoriesCount,
    type Forum
  } from '$lib/utils/client'
  import { getPageFromUrl } from '$lib/utils/tryParseInt'
  import type { FetchPageContext } from '$lib/types/fetchPageContext'

  let forumId: Forum['forumId'] = $derived(parseInt($page.params.forumId))
  let forum = $derived($forumStore.get(forumId))

  let perPage = $state(25)
  let currentPage: number = $state(getPageFromUrl($page.url))

  $effect(() => {
    if (forum !== undefined) return
    getForum<true>({ path: { forumId } }).then((v) =>
      forumStore.update((e) => {
        e.set(forumId, v.data)
        return e
      })
    )
  })

  $effect(() => {
    if ($store.categoryCount !== undefined) return
    getForumCategoriesCount({ path: { forumId: forumId } }).then(
      (v) => ($store.categoryCount = v.data)
    )
  })

  let fetchPageContext: FetchPageContext
  $effect(() => {
    const pageId = currentPage
    if ($store.pages[pageId] === undefined) {
      if (fetchPageContext) {
        if (fetchPageContext.pageId === pageId) return
        fetchPageContext.abortController.abort()
      }
      const abortController = new AbortController()
      const signal = abortController.signal
      fetchPageContext = { abortController, pageId }
      getForumCategories<true>({
        path: { forumId },
        query: { cursor: (pageId - 1) * perPage, limit: perPage },
        signal
      })
        .then((v) => {
          $store.pages[pageId] = v.data?.items
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
</script>

<h1 class="text-2xl font-bold">{forum?.title}</h1>
<Paginator {perPage} count={$store.categoryCount} />
{#if $store.pages[currentPage] != null}
  <div class="mt-4 rounded-lg border px-4 py-2">
    {#each $store.pages[currentPage] ?? [] as category, index}
      <CategoryView {category} />
      {#if index < ($store.pages[currentPage]?.length ?? 0) - 1}
        <Separator class="my-2" />
      {/if}
    {/each}
  </div>
{/if}
