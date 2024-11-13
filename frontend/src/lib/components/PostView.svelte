<script lang="ts">
  import * as Avatar from '$lib/components/ui/avatar'
  import { avatarUrl } from '$lib/config/env'
  import { formatTimestamp } from '$lib/utils/formatTimestamp'
  import { userLoader, userStore } from '$lib/stores/userStore.svelte'
  import { IconClockFilled } from '@tabler/icons-svelte'
  import type { Post } from '$lib/utils/client'

  let { post }: { post: Post } = $props()

  let creator = $derived(userStore.get(post.createdBy))

  $effect(() => {
    if (creator !== undefined) return
    const id = post.createdBy
    userLoader.load(id).then((user) => userStore.set(id, user))
  })
</script>

<article
  class="bg-muted/40 grid w-full grid-flow-row overflow-hidden sm:grid-cols-[10em,auto] sm:rounded-lg sm:border sm:bg-inherit"
>
  <div
    class="grid w-full auto-cols-max grid-flow-col items-center gap-x-1 border-r p-2 sm:grid-flow-row sm:items-start sm:gap-x-0"
  >
    <Avatar.Root class="size-8 justify-self-center sm:size-16">
      <Avatar.Image src="{avatarUrl}{post.createdBy}" alt="@shadcn" />
      <Avatar.Fallback>CN</Avatar.Fallback>
    </Avatar.Root>
    <div class="justify-self-center text-sm font-semibold">
      {creator?.username}
    </div>
    <time
      class="text-muted-foreground flex items-center gap-x-1 text-xs sm:mt-2"
      ><IconClockFilled class="size-3" />{formatTimestamp(
        creator?.createdAt
      )}</time
    >
  </div>
  <div>
    <header class="bg-muted/40 flex w-full p-2">
      <time class="text-muted-foreground text-sm"
        >{formatTimestamp(post.created)}</time
      ><span class="text-muted-foreground ml-auto text-sm">#{post.postId}</span>
    </header>
    <div class="p-2">{post.content}</div>
  </div>
</article>
