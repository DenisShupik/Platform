<script lang="ts" generics="T extends Record<string, unknown>">
	import * as FormPrimitive from 'formsnap'
	import PolicySelect from './policy-select.svelte'

	let {
		form,
		isNullable = true
	}: {
		form: FormPrimitive.FsSuperForm<T, any>
		isNullable?: boolean
	} = $props()
</script>

<PolicySelect {form} name="categoryCreatePolicyValue" title="Category create policy" {isNullable} />
