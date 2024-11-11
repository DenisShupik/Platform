<script lang="ts">
  import { page } from '$app/stores'
  import ForumView from '$lib/components/ForumView.svelte'
  import Paginator from '$lib/components/Paginator.svelte'
  import { getForums, getForumsCount, type Forum } from '$lib/utils/client'
  import { getPageFromUrl } from '$lib/utils/tryParseInt'

  let perPage = $state(25)
  let currentPage: number = $state(getPageFromUrl($page.url))

  let forumCount: number | undefined = $state()

  $effect(() => {
    if (forumCount === undefined) {
      getForumsCount<true>().then((v) => (forumCount = v.data))
    }
  })

  let pages: (Forum[] | undefined)[] = $state([])
  let fetchAbortController: AbortController | null = null
  $effect(() => {
    const pageId = currentPage
    if (pages[pageId] === undefined) {
      if (fetchAbortController) {
        fetchAbortController.abort()
      }
      const abortController = new AbortController()
      const signal = abortController.signal
      fetchAbortController = abortController
      getForums<true>({
        query: { cursor: (pageId - 1) * perPage, limit: perPage },
        signal
      })
        .then((v) => {
          pages[pageId] = v.data?.items
        })
        .catch((error) => {
          if (error.name !== 'AbortError') throw error
        })
        .finally(() => {
          if (fetchAbortController === abortController)
            fetchAbortController = null
        })
    }
  })
</script>

<Paginator bind:page={currentPage} {perPage} count={forumCount ?? 0} />
{#if pages[currentPage] != null}
  <div class="mt-2 space-y-4">
    {#each pages[currentPage] ?? [] as forum}
      <ForumView {forum} />
    {/each}
  </div>
{/if}
