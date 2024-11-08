<script lang="ts">
  import { Separator } from '$lib/components/ui/separator'
  import TopicStat from './ThreadStat.svelte'
  import PostStat from './PostStat.svelte'
  import { formatTimestamp } from '$lib/utils/formatTimestamp'
  import RouteLink from './ui/route-link/RouteLink.svelte'
  import { categoryStatsLoader } from '$lib/dataLoaders/categoryStatsLoader'
  import type { Category, CategoryStats } from '$lib/utils/client'

  let { category }: { category: Category } = $props()

  let stats: Omit<CategoryStats, 'categoryId'> | undefined = $state()

  $effect(() => {
    if (stats == null)
      categoryStatsLoader.load(category.categoryId).then(
        (v) =>
          (stats = v ?? {
            topicCount: 0,
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
    <div class="ml-2 grid h-full w-48 gap-y-1 text-sm font-medium">
      <RouteLink link="/" title="Разделы" />
      <time class="text-muted-foreground text-xs font-normal"
        >{formatTimestamp(category.created)}</time
      >
    </div>
  </div>
</div>
