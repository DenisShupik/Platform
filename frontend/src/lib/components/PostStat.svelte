<script lang="ts">
  import { getDeclension } from '$lib/getDeclension'
  import { Skeleton } from '$lib/components/ui/skeleton'

  const forms: [string, string, string] = [
    'сообщение',
    'сообщения',
    'сообщений'
  ]

  let { count }: { count?: number } = $props()
</script>

<dl class="grid place-items-center h-full w-16 gap-y-1 text-sm font-medium">
  {#if count == null}
    <Skeleton class="h-4 w-full" />
    <Skeleton class="h-4 w-full" />
  {:else}
    <dd>{count}</dd>
    <dt class="text-muted-foreground text-xs font-normal">{getDeclension(count, forms)}</dt>
  {/if}
</dl>
