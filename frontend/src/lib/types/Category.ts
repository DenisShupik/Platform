import type { Section } from './Section'

export interface Category extends Pick<Section, 'sectionId'> {
  categoryId: number
  title: string
  created: string
  createdBy: string
}
