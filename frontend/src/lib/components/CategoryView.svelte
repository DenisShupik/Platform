<script lang="ts" module>
  import { getCategoryPosts, type Post } from '$lib/utils/client'
  import DataLoader from 'dataloader'

  const latestCategoryPostLoader = new DataLoader<number, Post | null>(
    async (categoryIds) => {
      const posts = await getCategoryPosts<true>({
        path: { categoryIds },
        query: { latest: true }
      })
      const exists = new Map(
        posts.data?.map((item) => [item.categoryId, item.post])
      )
      return categoryIds.map((key) => {
        return exists.get(key) ?? null
      })
    },
    { maxBatchSize: 100, cache: false }
  )
</script>

<script lang="ts">
  import { Separator } from '$lib/components/ui/separator'
  import TopicStat from './ThreadStat.svelte'
  import PostStat from './PostStat.svelte'
  import RouteLink from './ui/route-link/RouteLink.svelte'
  import type { Category } from '$lib/utils/client'
  import LatestPostBlock from './latest-post-block.svelte'
  import { categoryThreadsCountLoader } from '$lib/dataLoaders/categoryThreadsCountLoader'
  import { categoryPostsCountLoader } from '$lib/dataLoaders/categoryPostsCountLoader'

  let { category }: { category: Category } = $props()
  let latestPost: Post | null | undefined = $state()
  let threadCount: number | undefined = $state()
  let postCount: number | undefined = $state()

  $effect(() => {
    if (latestPost === undefined) {
      latestCategoryPostLoader
        .load(category.categoryId)
        .then((v) => (latestPost = v))
    }
  })

  $effect(() => {
    if (threadCount == null)
      categoryThreadsCountLoader
        .load(category.categoryId)
        .then((v) => (threadCount = v))
  })

  $effect(() => {
    if (postCount == null)
      categoryPostsCountLoader
        .load(category.categoryId)
        .then((v) => (postCount = v))
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
