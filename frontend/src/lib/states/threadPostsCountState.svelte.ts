import DataLoader from 'dataloader'
import { SvelteMap } from 'svelte/reactivity'

import { getThreadPostsCount, type Thread } from '$lib/utils/client'

type IdType = Thread['threadId']
type MapType = SvelteMap<IdType, number>

export const threadPostsCountState = $state<MapType>(new SvelteMap())

export const threadPostsCountLoader = new DataLoader<IdType, number>(
  async (threadIds) => {
    const response = await getThreadPostsCount<true>({
      path: { threadIds }
    })
    const exists = new Map(
      Object.entries(response.data).map(([k, v]) => [parseInt(k), v])
    )
    return threadIds.map((key) => {
      return exists.get(key) ?? 0
    })
  },
  { maxBatchSize: 100, cache: false }
)
