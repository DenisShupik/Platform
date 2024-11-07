<script lang="ts">
  import * as Avatar from '$lib/components/ui/avatar'
  import { avatarUrl } from '$lib/env'
  import { formatTimestamp } from '$lib/utils/formatTimestamp'
  import type { Post } from '$lib/types/Post'
  import { userLoader, userStore } from '$lib/stores/userStore'
  import { IconUserFilled } from '@tabler/icons-svelte'

  let { post }: { post: Post } = $props()

  let creator = $derived($userStore.get(post.createdBy))

  $effect(() => {
    if (creator !== undefined) return
    const id = post.createdBy
    userLoader.load(id).then((user) =>
      userStore.update((e) => {
        e.set(id, user)
        return e
      })
    )
  })
</script>

<article
  class="grid h-32 w-full grid-cols-[10em,auto] overflow-hidden rounded-lg border"
>
  <div class="grid w-full grid-flow-row border-r p-2">
    <Avatar.Root class="h-16 w-16 justify-self-center">
      <Avatar.Image src="{avatarUrl}{creator?.userId}" alt="@shadcn" />
      <Avatar.Fallback>CN</Avatar.Fallback>
    </Avatar.Root>
    <div class="justify-self-center text-sm font-semibold">
      {creator?.username}
    </div>
    <time class="flex text-muted-foreground mt-2 text-xs gap-x-1 items-center"
      ><IconUserFilled class="size-4"/>{formatTimestamp(creator?.createdAt)}</time
    >
  </div>
  <div class="">
    <header class="bg-muted/40 flex w-full p-2">
      <time class="text-muted-foreground text-sm"
        >{formatTimestamp(post.created)}</time
      ><span class="text-muted-foreground ml-auto text-sm">#{post.postId}</span>
    </header>
    <div class="p-2">{post.content}</div>
  </div>
</article>
