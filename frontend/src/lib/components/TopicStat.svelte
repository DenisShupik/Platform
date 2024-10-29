<script lang="ts">
  import { getDeclension } from '$lib/getDeclension'
  const forms: [string, string, string] = ['тема', 'темы', 'тем']
  let { count }: { count: number } = $props()

  const title: string = $derived(getDeclension(count, forms))
</script>

<dl class="grid place-items-center h-full text-sm w-16">
  <dd>{count}</dd>
  <dt class="font-semibold">{title}</dt>
</dl>
