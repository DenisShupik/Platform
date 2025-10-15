<script lang="ts" generics="T extends Record<string, unknown>">
	import * as FormPrimitive from 'formsnap'
	import PolicySelect from './policy-select.svelte'
	import type { PolicyValue } from '$lib/utils/client'

	let {
		form,
		inheritedValue
	}: {
		form: FormPrimitive.FsSuperForm<T, any>
		inheritedValue: PolicyValue
	} = $props()
</script>

<PolicySelect {form} name="threadCreatePolicyValue" title="Thread create policy" {inheritedValue} />
