<script lang="ts">
	import * as Form from '$lib/components/ui/form'
	import { Input } from '$lib/components/ui/input'
	import { superForm } from 'sveltekit-superforms'
	import { zodClient } from 'sveltekit-superforms/adapters'
	import * as Card from '$lib/components/ui/card'
	import { zCreateThreadRequest } from '$lib/utils/client/zod.gen'
	import {
		createThread,
		getCategories,
		type CategoryId,
		type CategoryTitle
	} from '$lib/utils/client'
	import { authStore } from '$lib/client/auth-state.svelte'
	import { goto } from '$app/navigation'
	import * as Command from '$lib/components/ui/command'
	import * as Popover from '$lib/components/ui/popover'
	import Check from '@lucide/svelte/icons/check'
	import ChevronsUpDown from '@lucide/svelte/icons/chevrons-up-down'
	import { tick } from 'svelte'
	import { useId } from 'bits-ui'
	import { buttonVariants } from '$lib/components/ui/button'
	import { cn } from '$lib/utils'
	import { debounce } from 'lodash'

	const form = superForm(
		{ categoryId: '', title: '' },
		{
			SPA: true,
			validators: zodClient(zCreateThreadRequest),
			async onUpdate({ form }) {
				if (form.valid) {
					const result = await createThread<true>({
						body: { categoryId: form.data.categoryId, title: form.data.title },
						auth: $authStore.token
					})

					await goto(`/threads/${result.data}`)
				}
			}
		}
	)

	const { form: formData, enhance } = form

	let open = $state(false)

	function closeAndFocusTrigger(triggerId: string) {
		open = false
		tick().then(() => {
			document.getElementById(triggerId)?.focus()
		})
	}

	const triggerId = useId()

	let options: { label: CategoryTitle; value: CategoryId }[] = $state([])

	const fetchOptions = async (query: string) => {
		try {
			const response = await getCategories<true>({ query: { title: query } })
			options = response.data.map((category) => ({
				label: category.title,
				value: category.categoryId
			}))
		} catch (error) {
			console.error('Error fetching options:', error)
			options = []
		}
	}

	const debouncedFetch = debounce((query: string) => {
		if (query.trim().length >= 3) {
			fetchOptions(query)
		} else {
			options = []
		}
	}, 300)

	const handleInput = (event: Event) => {
		const target = event.target as HTMLInputElement
		debouncedFetch(target.value)
	}
</script>

<div class="flex flex-1 items-center justify-center">
	<form method="POST" use:enhance class="w-full md:max-w-xl">
		<Card.Root class="border-0 md:border">
			<Card.Header>
				<Card.Title>Создание темы</Card.Title>
				<Card.Description>Заполните форму для создания новой темы</Card.Description>
			</Card.Header>
			<Card.Content>
				<Form.Field {form} name="categoryId" class="flex flex-col">
					<Popover.Root bind:open>
						<Form.Control id={triggerId}>
							{#snippet children({ props })}
								<Form.Label>Категория</Form.Label>
								<Popover.Trigger
									class={cn(
										buttonVariants({ variant: 'outline' }),
										'w-[200px] justify-between',
										!$formData.categoryId && 'text-muted-foreground'
									)}
									role="combobox"
									{...props}
								>
									{options.find((f) => f.value === $formData.categoryId)?.label ?? 'Выберите тему'}
									<ChevronsUpDown class="opacity-50" />
								</Popover.Trigger>
								<input hidden value={$formData.categoryId} name={props.name} />
							{/snippet}
						</Form.Control>
						<Popover.Content class="w-[200px] p-0">
							<Command.Root>
								<Command.Input
									autofocus
									placeholder="Выберите тему"
									class="h-9"
									oninput={handleInput}
								/>
								<Command.List>
									<Command.Empty>Категории не найдены</Command.Empty>
									<Command.Group>
										{#each options as category (category.value)}
											<Command.Item
												value={category.label}
												onSelect={() => {
													$formData.categoryId = category.value
													closeAndFocusTrigger(triggerId)
												}}
											>
												{category.label}
												<!-- <Check
													class={cn(
														'ml-auto',
														category.value !== $formData.categoryId && 'text-transparent'
													)}
												/> -->
											</Command.Item>
										{/each}
									</Command.Group>
								</Command.List>
							</Command.Root>
						</Popover.Content>
					</Popover.Root>
					<Form.Description>
						This is the language that will be used in the dashboard.
					</Form.Description>
					<Form.FieldErrors />
				</Form.Field>
				<Form.Field {form} name="title">
					<Form.Control>
						{#snippet children({ props })}
							<Form.Label>Название темы</Form.Label>
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
