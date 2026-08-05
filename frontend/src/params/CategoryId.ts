import type { CategoryId } from '$lib/utils/client'
import { parseCategoryId } from '$lib/utils/value-object'
import type { ParamMatcher } from '@sveltejs/kit'

export const match = ((param: string): param is CategoryId => {
	return parseCategoryId(param) !== undefined
}) satisfies ParamMatcher
