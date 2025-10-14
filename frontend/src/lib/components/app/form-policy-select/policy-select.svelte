<script lang="ts" generics="T extends Record<string, unknown>, U extends FormPath<T>">
	import * as FormPrimitive from 'formsnap'
	import type { FormPath } from 'sveltekit-superforms'
	import * as Form from '$lib/components/ui/form'
	import * as Select from '$lib/components/ui/select'
	import { PolicyOptions, NullablePolicyOptions } from '.'

	let {
		form,
		name,
		title,
		isNullable = true
	}: {
		form: FormPrimitive.FsSuperForm<T, any>
		name: U
		title: string
		isNullable?: boolean
	} = $props()

	const { form: formData } = form
	const options = isNullable ? NullablePolicyOptions : PolicyOptions
</script>

<Form.Field {form} {name}>
	<Form.Control>
		{#snippet children({ props })}
			<Form.Label>{title}</Form.Label>
			<Select.Root type="single" bind:value={$formData[title]} name={props.name}>
				<Select.Trigger {...props} class="w-full">
					{options.find((f) => f.value === $formData[title])?.label ?? 'Select policy...'}
				</Select.Trigger>
				<Select.Content>
					{#each options as policy (policy.value)}
						<Select.Item value={policy.value} label={policy.label}>
							{policy.label}
						</Select.Item>
					{/each}
				</Select.Content>
			</Select.Root>
		{/snippet}
	</Form.Control>
	<Form.FieldErrors />
</Form.Field>
