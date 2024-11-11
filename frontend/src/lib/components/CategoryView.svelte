<script lang="ts" module>
  import { getPosts, type Post, type Thread } from '$lib/utils/client'
  import DataLoader from 'dataloader'

  const latestCategoryPostLoader = new DataLoader<number, Post | null>(
    async (ids) => {
      const posts = await getPosts<false>({
        query: { ids, filter: 'CategoryLatest' }
      })
      const exists = new Map(
        posts.data?.items.map((item) => [item.threadId, item])
      )
      return ids.map((key) => {
        return exists.get(key) ?? null
      })
    },
    { maxBatchSize: 100 }
  )
</script>

<script lang="ts">
  import { Separator } from '$lib/components/ui/separator'
  import TopicStat from './ThreadStat.svelte'
  import PostStat from './PostStat.svelte'
  import RouteLink from './ui/route-link/RouteLink.svelte'
  import { categoryStatsLoader } from '$lib/dataLoaders/categoryStatsLoader'
  import type { Category, CategoryStats } from '$lib/utils/client'
  import LatestPostBlock from './latest-post-block.svelte'

  let { category }: { category: Category } = $props()
  let latestPost: Post | null | undefined = $state()
  let stats: Omit<CategoryStats, 'categoryId'> | undefined = $state()

  $effect(() => {
    if (latestPost === undefined) {
      latestCategoryPostLoader
        .load(category.categoryId)
        .then((v) => (latestPost = v))
    }
  })

  $effect(() => {
    if (stats == null)
      categoryStatsLoader.load(category.categoryId).then(
        (v) =>
          (stats = v ?? {
            threadCount: 0,
            postCount: 0
          })
      )
  })
</script>

<div class="grid h-auto w-full grid-cols-[1fr,auto] items-center text-sm">
  <RouteLink
    link={`/categories/${category.categoryId}`}
    title={category.title}
  />
  <div class="grid grid-flow-col items-center">
    <TopicStat count={stats?.threadCount} class="hidden md:inline" />
    <Separator orientation="vertical" class="hidden md:inline" />
    <PostStat count={stats?.postCount} class="hidden md:inline" />
    <LatestPostBlock post={latestPost} />
  </div>
</div>
