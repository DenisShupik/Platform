import type { ThreadId } from '$lib/utils/client'
import { parseThreadId } from '$lib/utils/value-object'
import type { ParamMatcher } from '@sveltejs/kit'

export const match = ((param: string): param is ThreadId => {
	return parseThreadId(param) !== undefined
}) satisfies ParamMatcher
