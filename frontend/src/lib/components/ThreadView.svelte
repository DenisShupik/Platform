<script lang="ts">
  import { formatTimestamp } from '$lib/utils/formatTimestamp'
  import * as Avatar from '$lib/components/ui/avatar'
  import PostStat from './PostStat.svelte'
  import RouteLink from '$lib/components/ui/route-link/RouteLink.svelte'
  import { avatarUrl } from '$lib/config/env'
  import { IconClockFilled } from '@tabler/icons-svelte'
  import { type Post, type Thread } from '$lib/utils/client'
  import LatestPostBlock from './latest-post-block.svelte'
  import {
    threadPostsCountLoader,
    threadPostsCountState
  } from '$lib/states/threadPostsCountState.svelte'
  import { userLoader, userStore } from '$lib/states/userState.svelte'
  import {
    threadLatestPostLoader,
    threadLatestPostState
  } from '$lib/states/threadLatestPostState.svelte'

  let { thread }: { thread: Thread } = $props()
  let creator = $derived(userStore.get(thread.createdBy))
  let latestPost: Post | null | undefined = $derived(
    threadLatestPostState.get(thread.threadId)
  )
  let postCount: number | undefined = $derived(
    threadPostsCountState.get(thread.threadId)
  )

  $effect(() => {
    if (creator !== undefined) return
    const userId = thread.createdBy
    userLoader.load(userId).then((user) => userStore.set(userId, user))
  })

  $effect(() => {
    if (latestPost !== undefined) return
    const threadId = thread.threadId
    threadLatestPostLoader
      .load(threadId)
      .then((v) => threadLatestPostState.set(threadId, v))
  })

  $effect(() => {
    if (postCount !== undefined) return
    const threadId = thread.threadId
    threadPostsCountLoader
      .load(threadId)
      .then((v) => threadPostsCountState.set(threadId, v))
  })
</script>

<tr class="border">
  <td class="pl-4">
    <Avatar.Root class="h-full w-full p-2">
      <Avatar.Image src="{avatarUrl}{creator?.userId}" alt="@shadcn" />
      <Avatar.Fallback>{creator?.username}</Avatar.Fallback>
    </Avatar.Root>
  </td>
  <td class="border border-x-0 pl-2">
    <RouteLink
      link={`/threads/${thread.threadId}`}
      title={thread.title}
      class="font-semibold leading-none tracking-tight"
    />
    <p class="text-muted-foreground flex items-center gap-x-1 text-sm">
      <span>{creator?.username}</span><IconClockFilled
        class="inline size-3"
      /><time>{formatTimestamp(thread.created)}</time>
    </p>
  </td>
  <td class="hidden border md:table-cell"
    ><PostStat count={postCount} class="mx-auto" /></td
  >
  <td class="hidden border border-r-0 text-right md:table-cell">
    <LatestPostBlock post={latestPost} />
  </td>
</tr>
