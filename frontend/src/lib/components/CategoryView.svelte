<script lang="ts">
  import type { Category } from '$lib/types/Category'
  import { Separator } from '$lib/components/ui/separator'
  import TopicStat from './TopicStat.svelte'
  import { navigate } from '$lib/routeStore'

  let { category }: { category: Category } = $props()

  function preventDefault(fn) {
    return function (event) {
      event.preventDefault()
      fn.call(this, event)
    }
  }
</script>

<div class="w-full grid grid-cols-[1fr,auto] items-center text-sm h-auto">
  <a
    href={`/category/${category.categoryId}`}
    onclick={preventDefault(() => navigate(`/category/${category.categoryId}`))}
    >{category.title}e</a
  >

  <div class="ml-auto grid grid-flow-col items-center gap-x-4">
    <TopicStat count={10} />
    <Separator orientation="vertical" />
    <div>еще инфа</div>
  </div>
</div>
