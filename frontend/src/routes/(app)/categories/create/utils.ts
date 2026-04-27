import type { ForumDto, ForumId, ForumTitle } from '$lib/utils/client'

export type ForumData = {
	title: ForumTitle
}

export type Option = { key: ForumId; value: ForumData }

export function transformToOptions(forums: ForumDto[]) {
	const result = forums.reduce<Option[]>((out, e) => {
		out.push({
			key: e.forumId,
			value: {
				title: e.title
			}
		})
		return out
	}, [])

	return result
}