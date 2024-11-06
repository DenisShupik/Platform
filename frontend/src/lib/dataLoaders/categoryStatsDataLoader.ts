import type { CategoryStats } from '$lib/types/CategoryStats'
import { GET } from '$lib/utils/GET'
import DataLoader from 'dataloader'

export const categoryStatsDataLoader = new DataLoader<
  CategoryStats['categoryId'],
  CategoryStats | undefined
>(
  async (ids) => {
    const stats = await GET<CategoryStats[]>(
      `/categories/${ids.join(',')}/stats`
    )
    const exists = new Map(stats.map((item) => [item.categoryId, item]))
    return ids.map((key) => {
      return exists.get(key)
    })
  },
  { maxBatchSize: 100 }
)
