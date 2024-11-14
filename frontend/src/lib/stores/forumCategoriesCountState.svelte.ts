import DataLoader from 'dataloader'
import { SvelteMap } from 'svelte/reactivity'

import { type Forum, getForumCategoriesCount } from '$lib/utils/client'

type IdType = Forum['forumId']
type MapType = SvelteMap<IdType, number | undefined>

export const forumCategoriesCountState = $state<MapType>(new SvelteMap())

export const forumCategoriesCountLoader = new DataLoader<IdType, number>(
  async (forumIds) => {
    const response = await getForumCategoriesCount<true>({ path: { forumIds } })
    const exists = new Map(
      Object.entries(response.data).map(([k, v]) => [parseInt(k), v])
    )
    return forumIds.map((key) => {
      return exists.get(key) ?? 0
    })
  },
  { maxBatchSize: 100, cache: false }
)
