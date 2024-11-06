<script lang="ts">
  import { getDeclension } from '$lib/getDeclension'
  import { Skeleton } from '$lib/components/ui/skeleton'
  import { cn } from '$lib/utils'
  import type { WithoutChildren } from 'bits-ui'
  import type { HTMLAttributes } from 'svelte/elements'

  const forms: [string, string, string] = [
    'сообщение',
    'сообщения',
    'сообщений'
  ]

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
    'grid w-16 place-items-center place-self-center text-sm font-medium',
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
      {getDeclension(count, forms)}
    </p>
  {/if}
</div>
