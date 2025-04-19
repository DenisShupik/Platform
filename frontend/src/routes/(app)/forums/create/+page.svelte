<script lang="ts">
	import * as Form from '$lib/components/ui/form'
	import { Input } from '$lib/components/ui/input'
	import { superForm } from 'sveltekit-superforms'
	import { zodClient } from 'sveltekit-superforms/adapters'
	import * as Card from '$lib/components/ui/card'
	import { zCreateForumRequestBody } from '$lib/utils/client/zod.gen'
	import { createForum } from '$lib/utils/client'
	import { authStore } from '$lib/client/auth-state.svelte'
	import { goto } from '$app/navigation'

	const form = superForm(
		{ title: '' },
		{
			SPA: true,
			validators: zodClient(zCreateForumRequestBody),
			async onUpdate({ form }) {
				if (form.valid) {
					const result = await createForum<true>({
						body: { title: form.data.title },
						auth: $authStore.token
					})

					await goto(`/forums/${result.data}`)
				}
			}
		}
	)

	const { form: formData, enhance } = form
</script>

<div class="flex flex-1 items-center justify-center">
	<form method="POST" use:enhance class="w-full md:max-w-xl">
		<Card.Root class="border-0 md:border">
			<Card.Header>
				<Card.Title>Создание форума</Card.Title>
				<Card.Description>Заполните форму для создания нового форума</Card.Description>
			</Card.Header>
			<Card.Content>
				<Form.Field {form} name="title">
					<Form.Control>
						{#snippet children({ props })}
							<Form.Label>Название форума</Form.Label>
							<Input {...props} bind:value={$formData.title} />
						{/snippet}
					</Form.Control>
					<Form.FieldErrors />
				</Form.Field>
			</Card.Content>
			<Card.Footer class="flex justify-between">
				<Form.Button variant="outline">Отмена</Form.Button>
				<Form.Button>Создать</Form.Button>
			</Card.Footer>
		</Card.Root>
	</form>
</div>
