import DataLoader from 'dataloader'
import { SvelteMap } from 'svelte/reactivity'

import { type Category, getCategoryPosts, type Post } from '$lib/utils/client'

type IdType = Category['categoryId']
type MapType = SvelteMap<IdType, Post | null>

export const categoryLatestPostStore = $state<MapType>(new SvelteMap())

export const categoryLatestPostLoader = new DataLoader<IdType, Post | null>(
  async (categoryIds) => {
    const posts = await getCategoryPosts<true>({
      path: { categoryIds },
      query: { latest: true }
    })
    const exists = new Map(
      posts.data.map((item) => [item.categoryId, item.post])
    )
    return categoryIds.map((key) => {
      return exists.get(key) ?? null
    })
  },
  { maxBatchSize: 100, cache: false }
)
