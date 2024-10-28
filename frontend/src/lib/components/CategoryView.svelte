<script lang="ts">
  import type { Category } from '$lib/types/Category'

  let { category }: { category: Category } = $props()
</script>

<div class="w-full">{category.title}</div>
