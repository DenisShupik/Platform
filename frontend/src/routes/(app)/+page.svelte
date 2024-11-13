<script lang="ts" module>
  import { writable } from 'svelte/store'
  interface Store {
    forumCount?: number
    pages: (Forum[] | undefined)[]
  }

  export let store = writable<Store>({ pages: [] })

  export const invalidate = () => {
    store.update((s) => (s = { pages: [] }))
  }
</script>

<script lang="ts">
  import { page } from '$app/stores'
  import ForumView from '$lib/components/ForumView.svelte'
  import Paginator from '$lib/components/Paginator.svelte'
  import { getForums, getForumsCount, type Forum } from '$lib/utils/client'
  import { getPageFromUrl } from '$lib/utils/tryParseInt'
  import type { FetchPageContext } from '$lib/types/fetchPageContext'

  let perPage = $state(25)
  let currentPage: number = $derived(getPageFromUrl($page.url))

  $effect(() => {
    if ($store.forumCount === undefined) {
      getForumsCount<true>().then((v) => ($store.forumCount = v.data))
    }
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
      getForums<true>({
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

<Paginator {perPage} count={$store.forumCount ?? 0} />
{#if $store.pages[currentPage] != null}
  <div class="mt-2 space-y-4">
    {#each $store.pages[currentPage] ?? [] as forum}
      <ForumView {forum} />
    {/each}
  </div>
{/if}
