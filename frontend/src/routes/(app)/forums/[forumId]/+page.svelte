<script lang="ts">
  import { page } from '$app/stores'
  import CategoryView from '$lib/components/CategoryView.svelte'
  import Paginator from '$lib/components/Paginator.svelte'
  import { Separator } from '$lib/components/ui/separator'
  import { forumStore } from '$lib/stores/forumStore'
  import {
    getForum,
    getForumCategories,
    type Category,
    type Forum
  } from '$lib/utils/client'
  import { getPageFromUrl } from '$lib/utils/tryParseInt'
  import type { FetchPageContext } from '$lib/types/fetchPageContext'
  import {
    forumCategoriesCountLoader,
    forumCategoriesCountState
  } from '$lib/stores/forumCategoriesCountState.svelte'

  let forumId: Forum['forumId'] = $derived(parseInt($page.params.forumId))
  let forum = $derived($forumStore.get(forumId))

  let perPage = $state(25)
  let currentPage: number = $derived(getPageFromUrl($page.url))

  let categoryCount: number | undefined = $derived(
    forumCategoriesCountState.get(forumId)
  )

  let pageState: {
    pages: (Category[] | undefined)[]
  } = $state({ pages: [] })

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
    if (categoryCount !== undefined) return
    forumCategoriesCountLoader
      .load(forumId)
      .then((v) => forumCategoriesCountState.set(forumId, v))
  })

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
      getForumCategories<true>({
        path: { forumId },
        query: { cursor: (pageId - 1) * perPage, limit: perPage },
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
</script>

<h1 class="text-2xl font-bold">{forum?.title}</h1>
<Paginator {perPage} count={categoryCount} />
{#if pageState.pages[currentPage] != null}
  <div class="mt-4 rounded-lg border px-4 py-2">
    {#each pageState.pages[currentPage] ?? [] as category, index}
      <CategoryView {category} />
      {#if index < (pageState.pages[currentPage]?.length ?? 0) - 1}
        <Separator class="my-2" />
      {/if}
    {/each}
  </div>
{/if}
