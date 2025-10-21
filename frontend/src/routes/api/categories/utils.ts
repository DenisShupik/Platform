import type {
	CategoryDto,
	CategoryId,
	CategoryTitle,
	PolicyDto,
	PolicyNotFoundError,
	PolicyValue
} from '$lib/utils/client'

export type CategoryData = {
	title: CategoryTitle
	readPolicyValue: PolicyValue
	postCreatePolicyValue: PolicyValue
}

export type Option = { key: CategoryId; value: CategoryData }

export function transformToOptions(
	forums: CategoryDto[],
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
		const postCreatePolicyValue = policies[e.postCreatePolicyId]?.value?.value
		if (!postCreatePolicyValue) return out

		out.push({
			key: e.categoryId,
			value: {
				title: e.title,
				readPolicyValue,
				postCreatePolicyValue
			}
		})
		return out
	}, [])

	return result
}
