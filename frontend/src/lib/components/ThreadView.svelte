<script lang="ts" module>
  import { userLoader, userStore } from '$lib/stores/userStore'

  const latestThreadPostLoader = new DataLoader<number, Post | null>(
    async (ids) => {
      const posts = await getPosts<false>({
        query: { ids, filter: 'ThreadLatest' }
      })
      const exists = new Map(
        posts.data?.items.map((item) => [item.threadId, item])
      )
      return ids.map((key) => {
        return exists.get(key) ?? null
      })
    },
    { maxBatchSize: 100, cache: false }
  )
</script>

<script lang="ts">
  import { formatTimestamp } from '$lib/utils/formatTimestamp'
  import * as Avatar from '$lib/components/ui/avatar'
  import PostStat from './PostStat.svelte'
  import RouteLink from '$lib/components/ui/route-link/RouteLink.svelte'
  import DataLoader from 'dataloader'
  import { avatarUrl } from '$lib/config/env'
  import { IconClockFilled } from '@tabler/icons-svelte'
  import { postCountLoader } from '$lib/dataLoaders/postCountLoader'
  import { getPosts, type Post, type Thread } from '$lib/utils/client'
  import LatestPostBlock from './latest-post-block.svelte'

  let { thread }: { thread: Thread } = $props()
  let creator = $derived(
    thread === undefined ? undefined : $userStore.get(thread.createdBy)
  )
  let latestPost: Post | null | undefined = $state()
  let postCount: number | undefined = $state()

  $effect(() => {
    if (thread !== undefined && creator === undefined) {
      const id = thread.createdBy
      userLoader.load(id).then((user) =>
        userStore.update((e) => {
          e.set(id, user)
          return e
        })
      )
    }
  })

  $effect(() => {
    if (thread !== undefined && latestPost === undefined) {
      latestThreadPostLoader.load(thread.threadId).then((v) => (latestPost = v))
    }
  })

  $effect(() => {
    if (thread !== undefined && postCount === undefined) {
      postCountLoader.load(thread.threadId).then((v) => (postCount = v))
    }
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
