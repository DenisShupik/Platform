<script lang="ts">
  import { goto } from '$app/navigation'
  import * as Breadcrumb from '$lib/components/ui/breadcrumb'
  import { preventDefault } from '$lib/preventDefault'

  let { link, title }: { link: string; title: string } = $props()
</script>

<Breadcrumb.Link href={link} onclick={preventDefault(() => goto(link))}
  >{title}</Breadcrumb.Link
>
