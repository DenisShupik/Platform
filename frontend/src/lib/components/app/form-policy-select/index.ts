import { PolicyValue } from '$lib/utils/client'

export const PolicyOptions: { value: PolicyValue; label: string }[] = [
	{ value: PolicyValue.ANY, label: 'Any user' },
	{ value: PolicyValue.AUTHENTICATED, label: 'Authenticated user' },
	{ value: PolicyValue.GRANTED, label: 'User with grant' }
]

export const NullablePolicyOptions: { value: PolicyValue | null; label: string }[] = [
	{ value: null, label: 'Inherited' },
	...PolicyOptions
]

export { default as ReadPolicySelect } from './read-policy-select.svelte'
export { default as CategoryCreatePolicySelect } from './category-create-policy-select.svelte'
export { default as ThreadCreatePolicySelect } from './thread-create-policy-select.svelte'
export { default as PostCreatePolicySelect } from './post-create-policy-select.svelte'
