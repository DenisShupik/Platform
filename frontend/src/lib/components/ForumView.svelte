<script lang="ts" module>
  export const createforumsCategoriesLatestByPostPostMap = () =>
    new FetchMap<number, Category[] | null>(async (forumIds) => {
      const response = await getForumsCategoriesLatestByPost<true>({
        path: { forumIds }
      })
      const exists = new Map(
        Object.entries(response.data).map(([k, v]) => [parseInt(k), v])
      )
      return forumIds.map((key) => {
        return exists.get(key) ?? null
      })
    })
</script>

<script lang="ts">
  import * as Collapsible from '$lib/components/ui/collapsible'
  import { buttonVariants } from '$lib/components/ui/button'
  import { Separator } from '$lib/components/ui/separator'
  import CategoryView from './CategoryView.svelte'
  import CreateCategoryDialog from './dialogs/CreateCategoryDialog.svelte'
  import { IconChevronUp } from '@tabler/icons-svelte'
  import {
    getForumsCategoriesLatestByPost,
    type Category,
    type Forum
  } from '$lib/utils/client'
  import RouteLink from './ui/route-link/RouteLink.svelte'
  import { pluralize } from '$lib/utils/pluralize'
  import {
    forumCategoriesCountLoader,
    forumCategoriesCountState
  } from '$lib/states/forumCategoriesCountState.svelte'
  import { Skeleton } from './ui/skeleton'
  import { getContext } from 'svelte'
  import { FetchMap } from '$lib/utils/fetchMap'

  const forms: [string, string] = ['category', 'categories']

  var pageState: {
    forumsCategoriesLatestByPost:
      | FetchMap<number, Category[] | null>
      | undefined
  } = getContext('pageState')

  if (pageState.forumsCategoriesLatestByPost === undefined) {
    pageState.forumsCategoriesLatestByPost =
      createforumsCategoriesLatestByPostPostMap()
  }

  let { forum }: { forum: Forum } = $props()

  let categories: Category[] | null | undefined = $derived(
    pageState.forumsCategoriesLatestByPost?.get(forum.forumId)
  )

  let categoryCount: number | undefined = $derived(
    forumCategoriesCountState.get(forum.forumId)
  )

  $effect(() => {
    if (categoryCount !== undefined) return
    const forumId = forum.forumId
    forumCategoriesCountLoader
      .load(forumId)
      .then((v) => forumCategoriesCountState.set(forumId, v))
  })

  let isOpen = $state(true)
</script>

<Collapsible.Root
  class="bg-card text-card-foreground grid w-full rounded-lg border shadow-sm"
  bind:open={isOpen}
>
  <div class="bg-muted/40 flex h-10 items-center px-4">
    <RouteLink
      link="/forums/{forum.forumId}"
      title={forum.title}
      class="text-base font-semibold"
    />
    <div class="ml-auto flex items-center">
      {#if categoryCount === undefined}
        <Skeleton class="h-5 w-28" />
      {:else}
        <span class="w-28 whitespace-nowrap text-center text-sm font-light"
          >{categoryCount} {pluralize(categoryCount, forms)}</span
        >
      {/if}

      <CreateCategoryDialog
        forumId={forum.forumId}
        class={buttonVariants({ variant: 'ghost', class: 'h-8 gap-1' })}
      />
      <Collapsible.Trigger
        class={buttonVariants({
          variant: 'ghost',
          class: 'size-8 p-2'
        })}
      >
        <IconChevronUp
          class={`transition-transform duration-200 ${isOpen ? 'rotate-180' : 'rotate-0'}`}
        />
        <span class="sr-only">Toggle</span>
      </Collapsible.Trigger>
    </div>
  </div>
  {#if categories != null && categories.length !== 0}
    <Collapsible.Content class="px-4 py-2">
      {#each categories ?? [] as category, index}
        <CategoryView {category} />
        {#if index < (categories?.length ?? 0) - 1}
          <Separator class="my-2" />
        {/if}
      {/each}
    </Collapsible.Content>
  {/if}
</Collapsible.Root>
