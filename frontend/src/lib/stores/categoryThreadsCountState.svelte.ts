import DataLoader from 'dataloader'
import { SvelteMap } from 'svelte/reactivity'

import { type Category, getCategoryThreadsCount } from '$lib/utils/client'

type IdType = Category['categoryId']
type MapType = SvelteMap<IdType, number | undefined>

export const categoryThreadsCountState = $state<MapType>(new SvelteMap())

export const categoryThreadsCountLoader = new DataLoader<
  Category['categoryId'],
  number
>(
  async (categoryIds) => {
    const stats = await getCategoryThreadsCount<true>({ path: { categoryIds } })
    const exists = new Map(
      Object.entries(stats.data).map(([k, v]) => [parseInt(k), v])
    )
    return categoryIds.map((key) => {
      return exists.get(key) ?? 0
    })
  },
  { maxBatchSize: 100, cache: false }
)
