<script lang="ts">
  import type { Category } from '$lib/types/Category'
  import { Separator } from '$lib/components/ui/separator'

  let { category }: { category: Category } = $props()
</script>

<div class="w-full flex items-center text-sm">
  <div>{category.title}</div>
  <div class="ml-auto flex h-5 items-center space-x-4">
    <div>еще инфа</div>
    <Separator orientation="vertical" />
    <div>еще инфа</div>
  </div>
</div>