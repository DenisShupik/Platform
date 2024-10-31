<script lang="ts">
  import { preventDefault } from '$lib/preventDefault'
  import { navigate } from '$lib/stores/routeStore'
  import type { HTMLAnchorAttributes } from 'svelte/elements'

  let {
    link,
    title,
    class: className
  }: { link: string; title: string } & HTMLAnchorAttributes = $props()
</script>

<a class={className} href={link} onclick={preventDefault(() => navigate(link))}
  >{title}</a
>
