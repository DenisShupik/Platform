import { type Category, getCategoryPosts, type Post } from '$lib/utils/client'
import { FetchMap } from '$lib/utils/fetchMap'

type IdType = Category['categoryId']
type MapType = FetchMap<IdType, Post | null>

export const categoryLatestPostState = $state<MapType>(
  new FetchMap(async (categoryIds) => {
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
  })
)
