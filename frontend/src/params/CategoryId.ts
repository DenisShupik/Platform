import { zCategoryId } from '$lib/utils/client/zod.gen'
import type { ParamMatcher } from '@sveltejs/kit'

export const match: ParamMatcher = (value) => {
	return zCategoryId.safeParse(value).success
}
