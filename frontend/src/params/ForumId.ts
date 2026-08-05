import type { ForumId } from '$lib/utils/client'
import { parseForumId } from '$lib/utils/value-object'
import type { ParamMatcher } from '@sveltejs/kit'

export const match = ((param: string): param is ForumId => {
	return parseForumId(param) !== undefined
}) satisfies ParamMatcher
