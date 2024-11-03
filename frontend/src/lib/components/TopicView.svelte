<script lang="ts" module>
  import { userStore } from '$lib/stores/userStore'
  import type { KeysetPage } from '$lib/types/KeysetPage'
  import type { Post } from '$lib/types/Post'
  import type { User } from '$lib/types/User'
  const userLoader = new DataLoader<string, User>(async (ids) => {
    const users = await GET<KeysetPage<User>>(`/users?ids=${ids.join(',')}`)
    return users.items
  })

  const latestPostLoader = new DataLoader<number, Post | null>(
    async (ids) => {
      const posts = await GET<KeysetPage<Post>>(
        `/posts?topicIds=${ids.join(',')}&topicLatest=true`
      )
      const exists = new Map(posts.items.map((item) => [item.topicId, item]))
      return ids.map((key) => {
        return exists.get(key) ?? null
      })
    },
    { maxBatchSize: 100 }
  )
</script>

<script lang="ts">
  import { formatTimestamp } from '$lib/formatTimestamp'
  import type { Topic } from '$lib/types/Topic'
  import * as Avatar from '$lib/components/ui/avatar'
  import PostStat from './PostStat.svelte'
  import RouteLink from '$lib/components/ui/route-link/RouteLink.svelte'
  import { GET } from '$lib/GET'
  import DataLoader from 'dataloader'

  let { topic }: { topic: Topic } = $props()
  let creator = $derived(
    topic === undefined ? undefined : $userStore.get(topic.createdBy)
  )
  let latestPost: Post | null | undefined = $state()
  let latestPostAuthor = $derived(
    latestPost === undefined
      ? undefined
      : latestPost == null
        ? null
        : $userStore.get(latestPost.createdBy)
  )

  $effect(() => {
    if (topic !== undefined && creator === undefined) {
      const id = topic.createdBy
      userLoader.load(id).then((user) =>
        userStore.update((e) => {
          e.set(id, user)
          return e
        })
      )
    }
  })

  $effect(() => {
    if (topic !== undefined && latestPost === undefined) {
      latestPostLoader.load(topic.topicId).then((v) => (latestPost = v))
    }
  })

  $effect(() => {
    if (latestPost != null && latestPostAuthor === undefined) {
      const id = latestPost.createdBy
      userLoader.load(id).then((user) =>
        userStore.update((e) => {
          e.set(id, user)
          return e
        })
      )
    }
  })
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
      <span class="text-muted-foreground text-sm">{creator?.username}</span> ·
      <time class="text-muted-foreground text-sm"
        >{formatTimestamp(topic.created)}</time
      >
    </p>
  </td>
  <td class="hidden md:table-cell border"><PostStat count={0} /></td>
  <td class="hidden md:table-cell border border-r-0 text-right">
    <div class="text-sm">{latestPostAuthor?.username}</div>
    <time class="text-muted-foreground text-xs line-clamp-1"
      >{latestPost != null ? formatTimestamp(latestPost.created) : null}</time
    >
  </td>
  <td class="hidden md:table-cell">
    {#if latestPostAuthor != null}
      <Avatar.Root class="w-full h-full p-2">
        <Avatar.Image
          src="http://127.0.0.1:9000/avatars/user_1.jpg"
          alt="@shadcn"
        />
        <Avatar.Fallback>CN</Avatar.Fallback>
      </Avatar.Root>
    {/if}
  </td>
</tr>
