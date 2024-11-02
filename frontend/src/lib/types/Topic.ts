import type { Category } from './Category'

export interface Topic extends Pick<Category, 'categoryId'> {
  topicId: number
  title: string
  created: string
  createdBy: string
}
