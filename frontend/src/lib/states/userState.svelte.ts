import DataLoader from 'dataloader'
import { SvelteMap } from 'svelte/reactivity'

import { getUsers, type User } from '$lib/utils/client'

type IdType = User['userId']
type MapType = SvelteMap<IdType, User | null>

export const userStore = $state<MapType>(new SvelteMap())

export const userLoader = new DataLoader<IdType, User | null>(
  async (ids) => {
    const users = await getUsers<true>({ query: { ids } })
    const exists = new Map(users.data.items.map((item) => [item.userId, item]))
    return ids.map((key) => {
      return exists.get(key) ?? null
    })
  },
  { maxBatchSize: 100, cache: false }
)
