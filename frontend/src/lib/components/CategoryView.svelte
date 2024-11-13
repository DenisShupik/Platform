<script lang="ts">
  import { Separator } from '$lib/components/ui/separator'
  import TopicStat from './ThreadStat.svelte'
  import PostStat from './PostStat.svelte'
  import RouteLink from './ui/route-link/RouteLink.svelte'
  import type { Category, Post } from '$lib/utils/client'
  import LatestPostBlock from './latest-post-block.svelte'
  import {
    categoryLatestPostLoader,
    categoryLatestPostStore
  } from '$lib/stores/categoryLatestPostStore.svelte'
  import {
    categoryThreadsCountLoader,
    categoryThreadsCountState
  } from '$lib/stores/categoryThreadsCountState.svelte'
  import {
    categoryPostsCountLoader,
    categoryPostsCountStore
  } from '$lib/stores/categoryPostsCountStore.svelte'

  let { category }: { category: Category } = $props()
  let latestPost: Post | null | undefined = $derived(
    categoryLatestPostStore.get(category.categoryId)
  )
  let threadCount: number | undefined = $derived(
    categoryThreadsCountState.get(category.categoryId)
  )
  let postCount: number | undefined = $derived(
    categoryPostsCountStore.get(category.categoryId)
  )
  $effect(() => {
    if (latestPost !== undefined) return
    const categoryId = category.categoryId
    categoryLatestPostLoader
      .load(categoryId)
      .then((v) => categoryLatestPostStore.set(categoryId, v))
  })

  $effect(() => {
    if (threadCount !== undefined) return
    const categoryId = category.categoryId
    categoryThreadsCountLoader
      .load(category.categoryId)
      .then((v) => categoryThreadsCountState.set(categoryId, v))
  })

  $effect(() => {
    if (postCount !== undefined) return
    const categoryId = category.categoryId
    categoryPostsCountLoader
      .load(category.categoryId)
      .then((v) => categoryPostsCountStore.set(categoryId, v))
  })
</script>

<div class="grid h-auto w-full grid-cols-[1fr,auto] items-center text-sm">
  <RouteLink
    link={`/categories/${category.categoryId}`}
    title={category.title}
  />
  <div class="grid grid-flow-col items-center">
    <TopicStat count={threadCount} class="hidden md:grid" />
    <Separator orientation="vertical" class="hidden md:inline" />
    <PostStat count={postCount} class="hidden md:grid" />
    <LatestPostBlock post={latestPost} />
  </div>
</div>
