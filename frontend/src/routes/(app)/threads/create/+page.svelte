<script lang="ts">
	import * as Form from '$lib/components/ui/form'
	import { Input } from '$lib/components/ui/input'
	import { defaults, superForm } from 'sveltekit-superforms'
	import { valibot } from 'sveltekit-superforms/adapters'
	import * as Card from '$lib/components/ui/card'
	import {
		vCategoryId,
		vCategoryTitle,
		vCreateThreadRequestBody
	} from '$lib/utils/client/valibot.gen'
	import {
		createThread,
		getCategories,
		getCategory,
		type CategoryId,
		type CategoryTitle
	} from '$lib/utils/client'
	import { currentUser, login } from '$lib/client/current-user-state.svelte'
	import { goto } from '$app/navigation'
	import * as Command from '$lib/components/ui/command'
	import * as Popover from '$lib/components/ui/popover'
	import Check from '@lucide/svelte/icons/check'
	import ChevronsUpDown from '@lucide/svelte/icons/chevrons-up-down'
	import { onMount, tick } from 'svelte'
	import { useId } from 'bits-ui'
	import { buttonVariants } from '$lib/components/ui/button'
	import { cn } from '$lib/utils'
	import { debounce } from '$lib/utils/debounce'
	import { IconLoader2 } from '@tabler/icons-svelte'
	import { safeParse } from 'valibot'
	import { resolve } from '$app/paths'

	$effect(() => {
		if (!currentUser.user) {
			login()
		}
	})

	const form = superForm(defaults(valibot(vCreateThreadRequestBody)), {
		SPA: true,
		validators: valibot(vCreateThreadRequestBody),
		async onUpdate({ form }) {
			if (form.valid) {
				const result = await createThread<true>({
					body: { categoryId: form.data.categoryId, title: form.data.title },
					auth: currentUser.user?.token
				})

				await goto(resolve('/(app)/threads/[threadId=ThreadId]/draft', { threadId: result.data }))
			}
		}
	})

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
			const response = await getCategories<true>({
				query: { title: query },
				signal: controller.signal
			})
			options = response.data.map((category) => ({
				label: category.title,
				value: category.categoryId
			}))
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

	onMount(async () => {
		const currentUrl = new URL(window.location.href)
		const searchParam = currentUrl.searchParams.get('categoryId')
		const parseResult = safeParse(vCategoryId, searchParam)
		if (parseResult.success) {
			let categoryId = parseResult.output
			var category = await getCategory<true>({ path: { categoryId } })
			options = [
				{
					label: category.data.title,
					value: categoryId
				}
			]
			$formData.categoryId = categoryId
		}
	})
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
									{options?.find((f) => f.value === $formData.categoryId)?.label ??
										'Выберите категорию...'}
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
											<div class="flex items-center justify-center gap-2 pb-4 pt-5 text-sm">
												<IconLoader2 class="size-4 animate-spin" />
												<span>Загрузка...</span>
											</div>
										</Command.Loading>
									{/if}
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
												<Check
													class={cn(
														'ml-auto',
														category.value !== $formData.categoryId && 'text-transparent'
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
			</Card.Content>
			<Card.Footer class="flex justify-between">
				<Form.Button variant="outline">Отмена</Form.Button>
				<Form.Button>Создать</Form.Button>
			</Card.Footer>
		</Card.Root>
	</form>
</div>
