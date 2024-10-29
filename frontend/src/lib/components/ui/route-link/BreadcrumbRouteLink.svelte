<script lang="ts">
  import { preventDefault } from '$lib/preventDefault'
  import { navigate } from '$lib/routeStore'
  import * as Breadcrumb from '$lib/components/ui/breadcrumb'

  let { link, title }: { link: string; title: string } = $props()
</script>

<Breadcrumb.Link href={link} onclick={preventDefault(() => navigate(link))}
  >{title}</Breadcrumb.Link
>
