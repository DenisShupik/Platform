import type { Topic } from './Topic'

export interface Post extends Pick<Topic, 'topicId'> {
  postId: number
  content: string
  created: string
  createdBy: string
}
