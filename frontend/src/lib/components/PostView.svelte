<script lang="ts">
  import * as Avatar from '$lib/components/ui/avatar'
  import { formatTimestamp } from '$lib/formatTimestamp'
  import type { Post } from '$lib/types/Post'

  let { post }: { post: Post } = $props()
</script>

<article class="grid grid-cols-[8em,auto] w-full h-32 border rounded-lg overflow-hidden">
  <div class="grid w-full grid-flow-row border-r p-2">
    <Avatar.Root class="w-16 h-16 justify-self-center">
      <Avatar.Image
        src="http://127.0.0.1:9000/avatars/user_1.jpg"
        alt="@shadcn"
      />
      <Avatar.Fallback>CN</Avatar.Fallback>
    </Avatar.Root>
    <div class="text-sm font-semibold justify-self-center">{'Anon'}</div>
    <time class="text-sm text-muted-foreground">{'Anon'}</time>
  </div>
  <div class="">
    <header class="flex w-full p-2 bg-muted/40">
      <time class="text-sm text-muted-foreground"
        >{formatTimestamp(post.created)}</time
      ><span class="ml-auto text-sm text-muted-foreground">#{post.postId}</span>
    </header>
    <div class="p-2">
      {post.content}
    </div>
  </div>
</article>
