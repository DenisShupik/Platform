<script lang="ts">
  import type { Category } from '$lib/types/Category'
  import { Separator } from '$lib/components/ui/separator'
  import TopicStat from './TopicStat.svelte'
  import { navigate } from '$lib/routeStore'
  import { preventDefault } from '$lib/preventDefault'
  import { getContext, type Context } from './pages/SectionPage.svelte'
  import type { CategoryStats } from '$lib/types/CategoryStats'
  import PostStat from './PostStat.svelte'
  import { formatTimestamp } from '$lib/formatTimestamp'
  import RouteLink from './ui/route-link/RouteLink.svelte'

  let { category }: { category: Category } = $props()
  const context: Context = getContext()
  let stats: Omit<CategoryStats, 'categoryId'> | undefined = $derived(
    context.stats == null
      ? undefined
      : (context.stats.get(category.categoryId) ?? {
          topicCount: 0,
          postCount: 0
        })
  )

  let link = $derived(`/category/${category.categoryId}`)
</script>

<div class="w-full grid grid-cols-[1fr,auto] items-center text-sm h-auto">
  <RouteLink
    link={`/categories/${category.categoryId}`}
    title={category.title}
  />
  <div class="grid grid-flow-col items-center gap-x-2">
    <TopicStat count={stats?.topicCount} />
    <Separator orientation="vertical" />
    <PostStat count={stats?.postCount} />
    <div class="grid h-full w-48 gap-y-1 text-sm font-medium ml-2">
      <RouteLink link="/" title="Разделы" />
      <time class="text-muted-foreground text-xs font-normal"
        >{formatTimestamp(category.created)}</time
      >
    </div>
  </div>
</div>
