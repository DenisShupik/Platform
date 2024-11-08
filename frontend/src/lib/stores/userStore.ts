import DataLoader from 'dataloader'
import { writable } from 'svelte/store'

import type { User } from '$lib/types/User'
import { GET } from '$lib/utils/GET'

type IdType = User['userId']
type MapType = Map<IdType, User | null>

export const userStore = writable<MapType>(new Map())

export const userLoader = new DataLoader<IdType, User | null>(async (ids) => {
  const users = await GET<KeysetPage<User>>(`/users?ids=${ids.join(',')}`)
  const exists = new Map(users.items.map((item) => [item.userId, item]))
  return ids.map((key) => {
    return exists.get(key) ?? null
  })
})
