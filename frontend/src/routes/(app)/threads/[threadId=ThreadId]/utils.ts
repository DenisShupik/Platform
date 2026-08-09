import { vPostContent, vPostId } from '$lib/utils/client/valibot.gen'
import * as v from 'valibot'
import * as m from '$lib/paraglide/messages'

export function createPostSchema() {
	return v.pipe(
		v.object({
			postId: v.optional(vPostId),
			content: vPostContent,
			rowVersion: v.optional(v.pipe(v.number(), v.integer(), v.minValue(0), v.maxValue(4294967295)))
		}),
		v.check((input) => {
			if (input.postId !== undefined && input.rowVersion === undefined) {
				return false
			}
			return true
		}, m.validation_row_version())
	)
}
