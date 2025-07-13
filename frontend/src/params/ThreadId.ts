import { zThreadId } from '$lib/utils/client/zod.gen'
import type { ParamMatcher } from '@sveltejs/kit'

export const match: ParamMatcher = (value) => {
	return zThreadId.safeParse(value).success
}
