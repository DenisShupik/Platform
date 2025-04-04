<script lang="ts">
  import { page } from '$app/stores'
  import ForumView from '$lib/components/ForumView.svelte'
  import Paginator from '$lib/components/Paginator.svelte'
  import { getForums, getForumsCount, type Forum } from '$lib/utils/client'
  import { getPageFromUrl } from '$lib/utils/tryParseInt'
  import type { FetchPageContext } from '$lib/types/fetchPageContext'
  import { setContext } from 'svelte'

  let perPage = $state(10)
  let currentPage: number = $derived(getPageFromUrl($page.url))

  let pageState: {
    forumCount?: number
    pages?: Forum[]
  } = $state({})

  setContext('pageState', pageState)

  $effect(() => {
    if (pageState.forumCount !== undefined) return
    getForumsCount<true>().then((v) => (pageState.forumCount = v.data))
  })

  let fetchPageContext: FetchPageContext
  $effect(() => {
    const pageId = currentPage
    if (pageState.pages === undefined) {
      if (fetchPageContext) {
        if (fetchPageContext.pageId === pageId) return
        fetchPageContext.abortController.abort()
      }
      const abortController = new AbortController()
      const signal = abortController.signal
      fetchPageContext = { abortController, pageId }
      getForums<true>({
        query: {
          cursor: (pageId - 1) * perPage,
          limit: perPage,
          sort: '-latestPost'
        },
        signal
      })
        .then((v) => {
          pageState.pages = v.data?.items
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

<Paginator {perPage} count={pageState.forumCount ?? 0} />
{#if pageState.pages != null}
  <div class="mt-2 space-y-4">
    {#each pageState.pages ?? [] as forum}
      <ForumView {forum} />
    {/each}
  </div>
{/if}
