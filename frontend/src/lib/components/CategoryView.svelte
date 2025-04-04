<script lang="ts">
  import { Separator } from '$lib/components/ui/separator'
  import TopicStat from './ThreadStat.svelte'
  import PostStat from './PostStat.svelte'
  import RouteLink from './ui/route-link/RouteLink.svelte'
  import type { Category, Post } from '$lib/utils/client'
  import LatestPostBlock from './latest-post-block.svelte'
  import {
    createCategoryThreadsCountMap,
    type CategoryThreadsCountMapType
  } from '$lib/states/categoryThreadsCountMap.svelte'
  import { getContext } from 'svelte'
  import {
    createCategoryLatestPostMap,
    type CategoryLatestPostMapType
  } from '$lib/states/categoryLatestPostState.svelte'
  import {
    type CategoryPostsCountMapType,
    createCategoryPostsCountMap
  } from '$lib/states/categoryPostsCountState.svelte'

  var pageState: {
    categoryLatestPostMap?: CategoryLatestPostMapType
    categoryThreadsCountMap?: CategoryThreadsCountMapType
    categoryPostsCountMap?: CategoryPostsCountMapType
  } = getContext('pageState')

  if (pageState.categoryLatestPostMap === undefined) {
    pageState.categoryLatestPostMap = createCategoryLatestPostMap()
  }

  if (pageState.categoryThreadsCountMap === undefined) {
    pageState.categoryThreadsCountMap = createCategoryThreadsCountMap()
  }

  if (pageState.categoryPostsCountMap === undefined) {
    pageState.categoryPostsCountMap = createCategoryPostsCountMap()
  }

  let { category }: { category: Category } = $props()
  let latestPost: Post | null | undefined = $derived(
    pageState.categoryLatestPostMap.get(category.categoryId)
  )
  let threadCount: number | undefined = $derived(
    pageState.categoryThreadsCountMap.get(category.categoryId)
  )
  let postCount: number | undefined = $derived(
    pageState.categoryPostsCountMap.get(category.categoryId)
  )
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
