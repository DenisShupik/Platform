import DataLoader from 'dataloader'
import { SvelteMap } from 'svelte/reactivity'

import { getThreadPosts, type Post, type Thread } from '$lib/utils/client'

type IdType = Thread['threadId']
type MapType = SvelteMap<IdType, Post | null>

export const threadLatestPostState = $state<MapType>(new SvelteMap())

export const threadLatestPostLoader = new DataLoader<IdType, Post | null>(
  async (threadIds) => {
    const posts = await getThreadPosts<true>({
      path: { threadIds },
      query: { latest: true }
    })
    const exists = new Map(
      posts.data.items.map((item) => [item.threadId, item])
    )
    return threadIds.map((key) => {
      return exists.get(key) ?? null
    })
  },
  { maxBatchSize: 100, cache: false }
)
