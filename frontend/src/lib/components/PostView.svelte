<script lang="ts">
  import * as Avatar from '$lib/components/ui/avatar'
  import { avatarUrl } from '$lib/env'
  import { formatTimestamp } from '$lib/formatTimestamp'
  import { authStore } from '$lib/stores/authStore'
  import type { Post } from '$lib/types/Post'

  let { post }: { post: Post } = $props()
</script>

<article
  class="grid h-32 w-full grid-cols-[8em,auto] overflow-hidden rounded-lg border"
>
  <div class="grid w-full grid-flow-row border-r p-2">
    <Avatar.Root class="h-16 w-16 justify-self-center">
      <Avatar.Image src="{avatarUrl}{$authStore?.userId}" alt="@shadcn" />
      <Avatar.Fallback>CN</Avatar.Fallback>
    </Avatar.Root>
    <div class="justify-self-center text-sm font-semibold">{'Anon'}</div>
    <time class="text-muted-foreground text-sm">{'Anon'}</time>
  </div>
  <div class="">
    <header class="bg-muted/40 flex w-full p-2">
      <time class="text-muted-foreground text-sm"
        >{formatTimestamp(post.created)}</time
      ><span class="text-muted-foreground ml-auto text-sm">#{post.postId}</span>
    </header>
    <div class="p-2">
      {post.content}
    </div>
  </div>
</article>
