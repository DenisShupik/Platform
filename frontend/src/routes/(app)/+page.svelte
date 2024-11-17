<script lang="ts">
  import { page } from '$app/stores'
  import ForumView from '$lib/components/ForumView.svelte'
  import Paginator from '$lib/components/Paginator.svelte'
  import { getForums, getForumsCount, type Forum } from '$lib/utils/client'
  import { getPageFromUrl } from '$lib/utils/tryParseInt'
  import type { FetchPageContext } from '$lib/types/fetchPageContext'

  let perPage = $state(10)
  let currentPage: number = $derived(getPageFromUrl($page.url))

  let forumCount: number | undefined = $state()

  let pageState: {
    pages: (Forum[] | undefined)[]
  } = $state({ pages: [] })

  $effect(() => {
    if (forumCount !== undefined) return
    getForumsCount<true>().then((v) => (forumCount = v.data))
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
      getForums<true>({
        query: {
          cursor: (pageId - 1) * perPage,
          limit: perPage,
          sort: '-latestPost'
        },
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

<Paginator {perPage} count={forumCount ?? 0} />
{#if pageState.pages[currentPage] != null}
  <div class="mt-2 space-y-4">
    {#each pageState.pages[currentPage] ?? [] as forum}
      <ForumView {forum} />
    {/each}
  </div>
{/if}
