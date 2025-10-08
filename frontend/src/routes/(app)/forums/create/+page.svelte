<script lang="ts">
	import * as Form from '$lib/components/ui/form'
	import { Input } from '$lib/components/ui/input'
	import { defaults, superForm } from 'sveltekit-superforms'
	import * as Card from '$lib/components/ui/card'
	import * as Select from '$lib/components/ui/select'
	import { vCreateForumRequestBody } from '$lib/utils/client/valibot.gen'
	import { createForum, PolicyValue } from '$lib/utils/client'
	import { currentUser, login } from '$lib/client/current-user-state.svelte'
	import { goto } from '$app/navigation'
	import { resolve } from '$app/paths'
	import { valibot } from 'sveltekit-superforms/adapters'

	$effect(() => {
		if (!currentUser.user) {
			login()
		}
	})

	const policyOptions = [
		{ value: PolicyValue.ANY, label: 'Any user' },
		{ value: PolicyValue.AUTHENTICATED, label: 'Authenticated user' },
		{ value: PolicyValue.GRANTED, label: 'User with grant' }
	]

	const form = superForm(defaults(valibot(vCreateForumRequestBody)), {
		SPA: true,
		validators: valibot(vCreateForumRequestBody),
		async onUpdate({ form }) {
			if (form.valid) {
				const result = await createForum<true>({
					body: {
						title: form.data.title,
						accessPolicyValue: form.data.accessPolicyValue,
						categoryCreatePolicyValue: form.data.categoryCreatePolicyValue,
						threadCreatePolicyValue: form.data.threadCreatePolicyValue,
						postCreatePolicyValue: form.data.postCreatePolicyValue
					},
					auth: currentUser.user?.token
				})

				await goto(resolve('/(app)/forums/[forumId=ForumId]', { forumId: result.data }))
			}
		}
	})

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
				<Form.Field {form} name="accessPolicyValue">
					<Form.Control>
						{#snippet children({ props })}
							<Form.Label>Access policy</Form.Label>
							<Select.Root type="single" bind:value={$formData.accessPolicyValue} name={props.name}>
								<Select.Trigger {...props} class="w-full">
									{policyOptions.find((f) => f.value === $formData.accessPolicyValue)?.label ??
										'Select policy...'}
								</Select.Trigger>
								<Select.Content>
									{#each policyOptions as policy (policy.value)}
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
				<Form.Field {form} name="categoryCreatePolicyValue">
					<Form.Control>
						{#snippet children({ props })}
							<Form.Label>Category create policy</Form.Label>
							<Select.Root
								type="single"
								bind:value={$formData.categoryCreatePolicyValue}
								name={props.name}
							>
								<Select.Trigger {...props} class="w-full">
									{policyOptions.find((f) => f.value === $formData.categoryCreatePolicyValue)
										?.label ?? 'Select policy...'}
								</Select.Trigger>
								<Select.Content>
									{#each policyOptions as policy (policy.value)}
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
				<Form.Field {form} name="threadCreatePolicyValue">
					<Form.Control>
						{#snippet children({ props })}
							<Form.Label>Thread create policy</Form.Label>
							<Select.Root
								type="single"
								bind:value={$formData.threadCreatePolicyValue}
								name={props.name}
							>
								<Select.Trigger {...props} class="w-full">
									{policyOptions.find((f) => f.value === $formData.threadCreatePolicyValue)
										?.label ?? 'Select policy...'}
								</Select.Trigger>
								<Select.Content>
									{#each policyOptions as policy (policy.value)}
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
				<Form.Field {form} name="postCreatePolicyValue">
					<Form.Control>
						{#snippet children({ props })}
							<Form.Label>Post create policy</Form.Label>
							<Select.Root
								type="single"
								bind:value={$formData.postCreatePolicyValue}
								name={props.name}
							>
								<Select.Trigger {...props} class="w-full">
									{policyOptions.find((f) => f.value === $formData.postCreatePolicyValue)?.label ??
										'Select policy...'}
								</Select.Trigger>
								<Select.Content>
									{#each policyOptions as policy (policy.value)}
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
			</Card.Content>
			<Card.Footer class="flex justify-between">
				<Form.Button variant="outline">Отмена</Form.Button>
				<Form.Button>Создать</Form.Button>
			</Card.Footer>
		</Card.Root>
	</form>
</div>
