import DataLoader from 'dataloader'

import { getThreadPostsCount, type Thread } from '$lib/utils/client'

export const postCountLoader = new DataLoader<Thread['threadId'], number>(
  async (ids) => {
    const response = await getThreadPostsCount<true>({
      path: { threadIds: ids }
    })
    const exists = new Map(
      response.data.map((item) => [item.threadId, item.count])
    )
    return ids.map((key) => {
      return exists.get(key) ?? 0
    })
  },
  { maxBatchSize: 100, cache: false }
)
