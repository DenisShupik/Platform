import type {
	ForumDto,
	ForumId,
	ForumTitle,
	PolicyDto,
	PolicyNotFoundError,
	PolicyValue
} from '$lib/utils/client'

export type ForumData = {
	title: ForumTitle
	readPolicyValue: PolicyValue
	threadCreatePolicyValue: PolicyValue
	postCreatePolicyValue: PolicyValue
}

export type Option = { key: ForumId; value: ForumData }

export function transformToOptions(
	forums: ForumDto[],
	policies: {
		[key: string]: {
			value?: PolicyDto
			error?: {
				$type: 'PolicyNotFoundError'
			} & PolicyNotFoundError
		}
	}
) {
	const result = forums.reduce<Option[]>((out, e) => {
		const readPolicyValue = policies[e.readPolicyId]?.value?.value
		if (!readPolicyValue) return out
		const threadCreatePolicyValue = policies[e.threadCreatePolicyId]?.value?.value
		if (!threadCreatePolicyValue) return out
		const postCreatePolicyValue = policies[e.postCreatePolicyId]?.value?.value
		if (!postCreatePolicyValue) return out

		out.push({
			key: e.forumId,
			value: {
				title: e.title,
				readPolicyValue,
				threadCreatePolicyValue,
				postCreatePolicyValue
			}
		})
		return out
	}, [])

	return result
}
