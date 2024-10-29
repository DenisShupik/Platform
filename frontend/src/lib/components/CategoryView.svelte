<script lang="ts">
  import type { Category } from '$lib/types/Category'
  import { Separator } from '$lib/components/ui/separator'
  import TopicStat from './TopicStat.svelte'
  import { navigate } from '$lib/routeStore'
  import type { CategoryStats } from '$lib/types/CategoryStats'
  import { preventDefault } from '$lib/preventDefault'

  let {
    category,
    stats
  }: { category: Category; stats: Map<number, CategoryStats> } = $props()
</script>

<div class="w-full grid grid-cols-[1fr,auto] items-center text-sm h-auto">
  <a
    href={`/category/${category.categoryId}`}
    onclick={preventDefault(() => navigate(`/category/${category.categoryId}`))}
    >{category.title}</a
  >

  <div class="grid grid-flow-col items-center gap-x-2">
    <TopicStat count={stats.get(category.categoryId)?.topicCount ?? 0} />
    <Separator orientation="vertical" />
    <div>еще инфа</div>
  </div>
</div>
