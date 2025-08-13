import { vThreadId } from '$lib/utils/client/valibot.gen'
import type { ParamMatcher } from '@sveltejs/kit'
import { safeParse } from 'valibot'

export const match: ParamMatcher = (value) => {
	return safeParse(vThreadId, value).success
}
