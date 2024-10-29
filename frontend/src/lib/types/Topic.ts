import type { Category } from './Category'

export interface Topic extends Pick<Category, 'categoryId'> {
  postId: number
  title: string
  created: string
  createdBy: Date
}
