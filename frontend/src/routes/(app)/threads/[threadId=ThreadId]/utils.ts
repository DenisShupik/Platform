import { vPostContent, vPostId } from '$lib/utils/client/valibot.gen'
import { rowVersionSchema } from '$lib/utils/value-object'
import * as v from 'valibot'
import * as m from '$lib/paraglide/messages'

export function createPostSchema() {
	return v.pipe(
		v.object({
			postId: v.optional(vPostId),
			content: vPostContent,
			rowVersion: v.optional(rowVersionSchema)
		}),
		v.check((input) => {
			if (input.postId !== undefined && input.rowVersion === undefined) {
				return false
			}
			return true
		}, m.validation_row_version())
	)
}
