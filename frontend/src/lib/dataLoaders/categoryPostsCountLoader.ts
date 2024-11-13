import DataLoader from 'dataloader'

import { type Category, getCategoryPostsCount } from '$lib/utils/client'

export const categoryPostsCountLoader = new DataLoader<
  Category['categoryId'],
  number
>(
  async (categoryIds) => {
    const stats = await getCategoryPostsCount<true>({ path: { categoryIds } })
    const exists = new Map(
      Object.entries(stats.data).map(([k, v]) => [parseInt(k), v])
    )
    return categoryIds.map((key) => {
      return exists.get(key) ?? 0
    })
  },
  { maxBatchSize: 100, cache: false }
)
