import DataLoader from 'dataloader'

import { type CategoryStats, getCategoryStats } from '$lib/utils/client'

export const categoryStatsLoader = new DataLoader<
  CategoryStats['categoryId'],
  CategoryStats | null
>(
  async (categoryIds) => {
    const stats = await getCategoryStats<true>({ path: { categoryIds } })
    const exists = new Map(stats.data.map((item) => [item.categoryId, item]))
    return categoryIds.map((key) => {
      return exists.get(key) ?? null
    })
  },
  { maxBatchSize: 100 }
)
