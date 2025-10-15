<script lang="ts">
	import * as Form from '$lib/components/ui/form'
	import { Input } from '$lib/components/ui/input'
	import { superForm } from 'sveltekit-superforms'
	import * as Card from '$lib/components/ui/card'
	import { vCreateForumRequestBody } from '$lib/utils/client/valibot.gen'
	import { valibot } from 'sveltekit-superforms/adapters'
	import {
		ReadPolicySelect,
		CategoryCreatePolicySelect,
		PostCreatePolicySelect,
		ThreadCreatePolicySelect
	} from '$lib/components/app/form-policy-select'
	import type { PortalDto } from '$lib/utils/client'

	// TODO: сделать проверку кто, может создавать форумы

	let { data }: { data: any; portal: PortalDto } = $props()

	const form = superForm(data.form, { validators: valibot(vCreateForumRequestBody) })

	const { form: formData, enhance } = form
</script>

<div class="flex flex-1 items-center justify-center">
	<form method="POST" use:enhance class="w-full md:max-w-xl">
		<Card.Root class="border-0 md:border">
			<Card.Header>
				<Card.Title>Create forum</Card.Title>
				<Card.Description>Fill out the form to create a new forum</Card.Description>
			</Card.Header>
			<Card.Content>
				<Form.Field {form} name="title">
					<Form.Control>
						{#snippet children({ props })}
							<Form.Label>Forum title</Form.Label>
							<Input {...props} bind:value={$formData.title} />
						{/snippet}
					</Form.Control>
					<Form.FieldErrors />
				</Form.Field>
				<ReadPolicySelect {form} inheritedValue={data.portal.readPolicy} />
				<CategoryCreatePolicySelect {form} inheritedValue={data.portal.categotyCreatePolicy} />
				<ThreadCreatePolicySelect {form} inheritedValue={data.portal.threadCreatePolicy} />
				<PostCreatePolicySelect {form} inheritedValue={data.portal.postCreatePolicy} />
			</Card.Content>
			<Card.Footer class="flex justify-between">
				<Form.Button variant="outline">Cancel</Form.Button>
				<Form.Button>Create</Form.Button>
			</Card.Footer>
		</Card.Root>
	</form>
</div>
