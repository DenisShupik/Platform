<script lang="ts">
  import { Skeleton } from '$lib/components/ui/skeleton'
  import type { WithoutChildren } from 'bits-ui'
  import type { HTMLAttributes } from 'svelte/elements'
  import { cn } from '$lib/utils'
  import { pluralize } from '$lib/utils/pluralize'

  const forms: [string, string] = ['thread', 'threads']

  let {
    count,
    class: className,
    ...restProps
  }: { count?: number } & WithoutChildren<
    HTMLAttributes<HTMLDivElement>
  > = $props()
</script>

<div
  class={cn(
    'grid w-14 place-items-center place-self-center text-sm font-medium',
    className
  )}
  {...restProps}
>
  {#if count == null}
    <Skeleton class="h-4 w-full" />
    <Skeleton class="h-4 w-full" />
  {:else}
    <h1>{count}</h1>
    <p class="text-muted-foreground text-xs font-normal">
      {pluralize(count, forms)}
    </p>
  {/if}
</div>
