<script lang="ts">
  import { formatTimestamp } from '$lib/formatTimestamp'
  import type { Topic } from '$lib/types/Topic'
  import * as Avatar from '$lib/components/ui/avatar'
  import PostStat from './PostStat.svelte'
  import RouteLink from '$lib/components/ui/route-link/RouteLink.svelte'

  let { topic }: { topic: Topic } = $props()
</script>

<tr class="border">
  <td>
    <Avatar.Root class="w-full h-full p-2">
      <Avatar.Image
        src="http://127.0.0.1:9000/avatars/user_1.jpg"
        alt="@shadcn"
      />
      <Avatar.Fallback>CN</Avatar.Fallback>
    </Avatar.Root>
  </td>
  <td class="pl-2 border border-x-0">
    <RouteLink
      link={`/topics/${topic.topicId}`}
      title={topic.title}
      class="font-semibold leading-none tracking-tight"
    />
    <p>
      <span class="text-muted-foreground text-sm">Anon</span>
      <time class="text-muted-foreground text-sm"
        >{formatTimestamp(topic.created)}</time
      >
    </p>
  </td>
  <td class="hidden md:table-cell border"><PostStat count={0} /></td>
  <td class="hidden md:table-cell border border-r-0 pr-2">
    <div class="text-right text-muted-foreground text-sm">{topic.title}</div>
    <p class="text-right">
      <span class="text-muted-foreground text-sm">Anon</span>
      <time class="text-muted-foreground text-sm"
        >{formatTimestamp(topic.created)}</time
      >
    </p>
  </td>
  <td class="hidden md:table-cell">
    <Avatar.Root class="w-full h-full p-2">
      <Avatar.Image
        src="http://127.0.0.1:9000/avatars/user_1.jpg"
        alt="@shadcn"
      />
      <Avatar.Fallback>CN</Avatar.Fallback>
    </Avatar.Root>
  </td></tr
>
