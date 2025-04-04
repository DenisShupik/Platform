import { type Category, getCategoryThreadsCount } from '$lib/utils/client'
import { FetchMap } from '$lib/utils/fetchMap'

type IdType = Category['categoryId']
export type CategoryThreadsCountMapType = FetchMap<IdType, number>

export const createCategoryThreadsCountMap: () => CategoryThreadsCountMapType =
  () =>
    new FetchMap(async (categoryIds) => {
      const stats = await getCategoryThreadsCount<true>({
        path: { categoryIds }
      })
      const exists = new Map(
        Object.entries(stats.data).map(([k, v]) => [parseInt(k), v])
      )
      return categoryIds.map((key) => {
        return exists.get(key) ?? 0
      })
    })
