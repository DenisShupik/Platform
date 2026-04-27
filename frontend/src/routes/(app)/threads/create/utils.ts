import type { CategoryDto, CategoryId, CategoryTitle } from '$lib/utils/client'

export type CategoryData = {
	title: CategoryTitle
}

export type Option = { key: CategoryId; value: CategoryData }

export function transformToOptions(forums: CategoryDto[]) {
	const result = forums.reduce<Option[]>((out, e) => {
		out.push({
			key: e.categoryId,
			value: {
				title: e.title
			}
		})
		return out
	}, [])

	return result
}