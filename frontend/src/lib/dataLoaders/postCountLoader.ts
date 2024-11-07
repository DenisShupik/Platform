import DataLoader from 'dataloader'

import type { Topic } from '$lib/types/Topic'
import { GET } from '$lib/utils/GET'

export const postCountLoader = new DataLoader<Topic['topicId'], number>(
  async (ids) => {
    const response = await GET<(Pick<Topic, 'topicId'> & { count: number })[]>(
      `/topics/${ids.join(',')}/posts/count`
    )
    const exists = new Map(response.map((item) => [item.topicId, item.count]))
    return ids.map((key) => {
      return exists.get(key) ?? 0
    })
  },
  { maxBatchSize: 100 }
)
