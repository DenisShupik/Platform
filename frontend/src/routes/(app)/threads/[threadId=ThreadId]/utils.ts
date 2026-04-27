import { vPostContent, vPostId } from '$lib/utils/client/valibot.gen'
import * as v from 'valibot'

export const postSchema = v.pipe(
	v.object({
		postId: v.optional(vPostId),
		content: vPostContent,
		rowVersion: v.optional(
			v.pipe(
				v.number(),
				v.integer(),
				v.minValue(0, 'Invalid value: Expected uint32 to be >= 0'),
				v.maxValue(4294967295, 'Invalid value: Expected uint32 to be <= 4294967295')
			)
		)
	}),
	v.check((input) => {
		if (input.postId !== undefined && input.rowVersion === undefined) {
			return false
		}
		return true
	}, 'При редактировании необходимо указать версию записи (rowVersion)')
)
