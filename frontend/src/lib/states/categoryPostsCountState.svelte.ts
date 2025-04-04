import { type Category, getCategoryPostsCount } from '$lib/utils/client'
import { FetchMap } from '$lib/utils/fetchMap'

type IdType = Category['categoryId']
export type CategoryPostsCountMapType = FetchMap<IdType, number>

export const createCategoryPostsCountMap: () => CategoryPostsCountMapType = () =>
  new FetchMap(async (categoryIds) => {
    const stats = await getCategoryPostsCount<true>({ path: { categoryIds } })
    const exists = new Map(
      Object.entries(stats.data).map(([k, v]) => [parseInt(k), v])
    )
    return categoryIds.map((key) => {
      return exists.get(key) ?? 0
    })
  })
