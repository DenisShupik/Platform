<script lang="ts">
  import { formatTimestamp } from '$lib/formatTimestamp'
  import type { Topic } from '$lib/types/Topic'
  import * as Avatar from '$lib/components/ui/avatar/index.js'
  import PostStat from './PostStat.svelte'

  let { topic }: { topic: Topic } = $props()
</script>

<div
  class="grid grid-cols-[4rem_1fr] md:grid-cols-[4rem_1fr_6rem_12rem_3em] border-t border-b items-center"
>
  <Avatar.Root class="w-16 h-16 p-2">
    <Avatar.Image
      src="http://127.0.0.1:9000/avatars/user_1.jpg"
      alt="@shadcn"
    />
    <Avatar.Fallback>CN</Avatar.Fallback>
  </Avatar.Root>
  <div class="ml-2">
    <div class="font-semibold leading-none tracking-tight">{topic.title}</div>
    <p>
      <span class="text-muted-foreground text-sm">Anon</span>
      <time class="text-muted-foreground text-sm"
        >{formatTimestamp(topic.created)}</time
      >
    </p>
  </div>
  <PostStat count={0} class="hidden md:block border-l" />
  <div class="hidden md:block mr-2 border-l">
    <div class="text-right text-muted-foreground text-sm">{topic.title}</div>
    <p class="text-right">
      <span class="text-muted-foreground text-sm">Anon</span>
      <time class="text-muted-foreground text-sm"
        >{formatTimestamp(topic.created)}</time
      >
    </p>
  </div>

  <Avatar.Root class="hidden md:block w-12 h-12 p-2">
    <Avatar.Image
      src="http://127.0.0.1:9000/avatars/user_1.jpg"
      alt="@shadcn"
    />
    <Avatar.Fallback>CN</Avatar.Fallback>
  </Avatar.Root>
</div>
