import DataLoader from 'dataloader'
import { writable } from 'svelte/store'

import { getUsers, type User } from '$lib/utils/client'

type IdType = User['userId']
type MapType = Map<IdType, User | null>

export const userStore = writable<MapType>(new Map())

export const userLoader = new DataLoader<IdType, User | null>(async (ids) => {
  const users = await getUsers<true>({ query: { ids } })
  const exists = new Map(users.data.items.map((item) => [item.userId, item]))
  return ids.map((key) => {
    return exists.get(key) ?? null
  })
})
