import { vForumId } from '$lib/utils/client/valibot.gen'
import type { ParamMatcher } from '@sveltejs/kit'
import { safeParse } from 'valibot'

export const match: ParamMatcher = (value) => {
	return safeParse(vForumId, value).success
}
