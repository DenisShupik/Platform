<script lang="ts">
  import { getDeclension } from '$lib/utils/getDeclension'
  import { Skeleton } from '$lib/components/ui/skeleton'

  const forms: [string, string, string] = ['тема', 'темы', 'тем']

  let { count }: { count?: number } = $props()
</script>

<dl class="grid w-16 place-items-center text-sm font-medium">
  {#if count == null}
    <Skeleton class="h-4 w-full" />
    <Skeleton class="h-4 w-full" />
  {:else}
    <dd>{count}</dd>
    <dt class="text-muted-foreground text-xs font-normal">
      {getDeclension(count, forms)}
    </dt>
  {/if}
</dl>
