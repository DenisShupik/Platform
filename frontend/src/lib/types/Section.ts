import type { Category } from './Category'

export interface Section {
  sectionId: number
  title: string
  created: string
  createdBy: string
  categories: Category[] | null
}
