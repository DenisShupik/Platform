<script lang="ts">
  import { preventDefault } from '$lib/preventDefault'
  import { navigate } from '$lib/stores/routeStore'

  let { link, title }: { link: string; title: string } = $props()
</script>

<a href={link} onclick={preventDefault(() => navigate(link))}>{title}</a>
