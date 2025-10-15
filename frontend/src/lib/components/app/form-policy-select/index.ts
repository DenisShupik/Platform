import { PolicyValue } from '$lib/utils/client'

const LABELS: Record<PolicyValue, string> = {
	[PolicyValue.ANY]: 'Any user',
	[PolicyValue.AUTHENTICATED]: 'Authenticated user',
	[PolicyValue.GRANTED]: 'User with grant'
}

function createOption(value: PolicyValue | null, inheritedValue?: PolicyValue) {
	const label = value === null 
		? `Inherited${inheritedValue ? ` (${LABELS[inheritedValue]})` : ''}`
		: LABELS[value]
	
	return { value, label }
}

const OPTIONS = {
	[PolicyValue.AUTHENTICATED]: createOption(PolicyValue.AUTHENTICATED),
	[PolicyValue.GRANTED]: createOption(PolicyValue.GRANTED)
}

export function getPolicyOptions(inheritedValue: PolicyValue) {
	switch (inheritedValue) {
		case PolicyValue.ANY:
			return [
				createOption(null, inheritedValue),
				OPTIONS[PolicyValue.AUTHENTICATED],
				OPTIONS[PolicyValue.GRANTED]
			]
		case PolicyValue.AUTHENTICATED:
		case PolicyValue.GRANTED:
			return [createOption(null, inheritedValue), OPTIONS[PolicyValue.GRANTED]]
		default:
			return []
	}
}

export { default as ReadPolicySelect } from './read-policy-select.svelte'
export { default as CategoryCreatePolicySelect } from './category-create-policy-select.svelte'
export { default as ThreadCreatePolicySelect } from './thread-create-policy-select.svelte'
export { default as PostCreatePolicySelect } from './post-create-policy-select.svelte'