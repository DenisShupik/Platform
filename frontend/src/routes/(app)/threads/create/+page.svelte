<script lang="ts">
	import * as Form from '$lib/components/ui/form'
	import { Input } from '$lib/components/ui/input'
	import { superForm } from 'sveltekit-superforms'
	import { valibot } from 'sveltekit-superforms/adapters'
	import * as Card from '$lib/components/ui/card'
	import { vCategoryTitle, vCreateThreadRequestBody } from '$lib/utils/client/valibot.gen'
	import { PolicyValue } from '$lib/utils/client'
	import * as Command from '$lib/components/ui/command'
	import * as Popover from '$lib/components/ui/popover'
	import Check from '@lucide/svelte/icons/check'
	import ChevronsUpDown from '@lucide/svelte/icons/chevrons-up-down'
	import { tick } from 'svelte'
	import { useId } from 'bits-ui'
	import { buttonVariants } from '$lib/components/ui/button'
	import { cn } from '$lib/utils'
	import { debounce } from '$lib/utils/debounce'
	import { IconLoader2 } from '@tabler/icons-svelte'
	import { safeParse } from 'valibot'
	import type { Option } from '../../../api/categories/utils'
	import { PostCreatePolicySelect, ReadPolicySelect } from '$lib/components/app/form-policy-select'

	let { data } = $props()

	const form = superForm(data.form, { validators: valibot(vCreateThreadRequestBody) })

	const { form: formData, enhance } = form

	let open = $state(false)

	function closeAndFocusTrigger(triggerId: string) {
		open = false
		tick().then(() => {
			document.getElementById(triggerId)?.focus()
		})
	}

	const triggerId = useId()

	let options: Option[] = $state(data.options)

	let loading = $state(false)
	let currentAbort = $state<AbortController | null>(null)

	const fetchOptions = async (query: string) => {
		if (currentAbort) {
			currentAbort.abort()
			currentAbort = null
		}

		query = query.trim()

		const result = safeParse(vCategoryTitle, query)

		if (!result.success) {
			loading = false
			return
		}

		loading = true
		const controller = new AbortController()
		currentAbort = controller

		try {
			const params = new URLSearchParams({ title: query })

			const response = await fetch(`/api/categories/?${params}`, {
				method: 'GET',
				credentials: 'include'
			})
			if (!response.ok) {
				throw new Error(`HTTP error! status: ${response.status}`)
			}
			options = await response.json()
		} catch (error: any) {
			if (error.name === 'AbortError') {
				console.log('Запрос отменён')
			} else {
				console.error('Ошибка при поиске:', error)
				options = []
			}
		} finally {
			loading = false
			if (currentAbort === controller) {
				currentAbort = null
			}
		}
	}

	let searchInputValue = $state('')

	const debouncedSearch = debounce(fetchOptions, 300)

	$effect(() => {
		debouncedSearch(searchInputValue)
	})

	let selected = $derived(options.find((f) => f.key === $formData.categoryId)?.value)
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
										'w-full justify-between',
										!$formData.categoryId && 'text-muted-foreground'
									)}
									role="combobox"
									{...props}
								>
									{selected?.title ?? 'Выберите категорию...'}
									<ChevronsUpDown class="opacity-50" />
								</Popover.Trigger>
								<input hidden value={$formData.categoryId} name={props.name} />
							{/snippet}
						</Form.Control>
						<Popover.Content class="w-[512px] p-0">
							<Command.Root shouldFilter={false}>
								<Command.Input
									autofocus
									placeholder="Введите название категории..."
									class="h-9"
									bind:value={searchInputValue}
								/>
								<Command.List>
									{#if !loading}
										<Command.Empty>Категории не найдены</Command.Empty>
									{/if}
									{#if loading}
										<Command.Loading>
											<div class="flex items-center justify-center gap-2 pt-5 pb-4 text-sm">
												<IconLoader2 class="size-4 animate-spin" />
												<span>Загрузка...</span>
											</div>
										</Command.Loading>
									{/if}
									<Command.Group>
										{#each options as category (category.key)}
											<Command.Item
												value={category.value.title}
												onSelect={() => {
													$formData.categoryId = category.key
													closeAndFocusTrigger(triggerId)
												}}
											>
												{category.value.title}
												<Check
													class={cn(
														'ml-auto',
														category.key !== $formData.categoryId && 'text-transparent'
													)}
												/>
											</Command.Item>
										{/each}
									</Command.Group>
								</Command.List>
							</Command.Root>
						</Popover.Content>
					</Popover.Root>
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
				<ReadPolicySelect {form} inheritedValue={selected?.readPolicyValue ?? PolicyValue.ANY} />
				<PostCreatePolicySelect
					{form}
					inheritedValue={selected?.postCreatePolicyValue ?? PolicyValue.ANY}
				/>
			</Card.Content>
			<Card.Footer class="flex justify-between">
				<Form.Button variant="outline">Отмена</Form.Button>
				<Form.Button>Создать</Form.Button>
			</Card.Footer>
		</Card.Root>
	</form>
</div>
