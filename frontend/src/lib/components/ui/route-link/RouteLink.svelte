<script lang="ts">
  import { goto } from '$app/navigation'
  import { preventDefault } from '$lib/utils/preventDefault'
  import type { HTMLAnchorAttributes } from 'svelte/elements'

  let {
    link,
    title,
    class: className
  }: { link: string; title: string } & HTMLAnchorAttributes = $props()
</script>

<a class={className} href={link} onclick={preventDefault(() => goto(link))}
  >{title}</a
>
