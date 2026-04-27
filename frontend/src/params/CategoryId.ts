import type { CategoryId } from '$lib/utils/client'
import { vCategoryId } from '$lib/utils/client/valibot.gen'
import type { ParamMatcher } from '@sveltejs/kit'
import { safeParse } from 'valibot'

export const match = ((param: string): param is CategoryId => {
	return safeParse(vCategoryId, param).success
}) satisfies ParamMatcher
