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
    type Category,
    type Forum
  } from '$lib/utils/client'
  import { getPageFromUrl } from '$lib/utils/tryParseInt'

  let forumId: Forum['forumId'] = $derived(parseInt($page.params.forumId))
  let forum = $derived($forumStore.get(forumId))

  let perPage = $state(25)
  let currentPage: number = $state(getPageFromUrl($page.url))

  let categoryCount: number | undefined = $state()

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
    getForumCategoriesCount({ path: { forumId: forumId } }).then(
      (v) => (categoryCount = v.data)
    )
  })

  let pages: (Category[] | undefined)[] = $state([])
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
      getForumCategories<true>({
        path: { forumId },
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

<h1 class="text-2xl font-bold">{forum?.title}</h1>
<Paginator bind:page={currentPage} {perPage} count={categoryCount} />
{#if pages[currentPage] != null}
  <div class="mt-4 rounded-lg border px-4 py-2">
    {#each pages[currentPage] ?? [] as category, index}
      <CategoryView {category} />
      {#if index < (pages[currentPage]?.length ?? 0) - 1}
        <Separator class="my-2" />
      {/if}
    {/each}
  </div>
{/if}
