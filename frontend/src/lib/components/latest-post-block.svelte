<script lang="ts">
  import { formatTimestamp } from '$lib/utils/formatTimestamp'
  import * as Avatar from '$lib/components/ui/avatar'
  import type { Post } from '$lib/utils/client'
  import { userLoader, userStore } from '$lib/stores/userStore.svelte'
  import { avatarUrl } from '$lib/config/env'

  let { post }: { post: Post | null | undefined } = $props()

  let author = $derived(
    post === undefined
      ? undefined
      : post == null
        ? null
        : userStore.get(post.createdBy)
  )

  $effect(() => {
    if (post == null || author !== undefined) return
    const id = post.createdBy
    userLoader.load(id).then((user) => userStore.set(id, user))
  })
</script>

<div class="grid w-48 grid-cols-[auto,3em] gap-x-1">
  <div class="grid w-full self-center text-right text-sm font-medium">
    <div>{author?.username}</div>
    <time class="text-muted-foreground line-clamp-1 text-xs"
      >{post != null ? formatTimestamp(post.created) : null}</time
    >
  </div>
  {#if author != null}
    <Avatar.Root class="h-full w-full p-2">
      <Avatar.Image src="{avatarUrl}{author.userId}" alt="@shadcn" />
      <Avatar.Fallback>{author.username}</Avatar.Fallback>
    </Avatar.Root>
  {/if}
</div>
