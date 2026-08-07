<script lang="ts">
	import * as Form from '$lib/components/ui/form'
	import { Input } from '$lib/components/ui/input'
	import { superForm } from 'sveltekit-superforms'
	import { valibot } from 'sveltekit-superforms/adapters'
	import * as Card from '$lib/components/ui/card'
	import { vCreateThreadRequestBody } from '$lib/utils/client/valibot.gen'
	import * as Command from '$lib/components/ui/command'
	import * as Popover from '$lib/components/ui/popover'
	import Check from '@lucide/svelte/icons/check'
	import ChevronsUpDown from '@lucide/svelte/icons/chevrons-up-down'
	import { tick, untrack } from 'svelte'
	import { useId } from 'bits-ui'
	import { Button, buttonVariants } from '$lib/components/ui/button'
	import { cn } from '$lib/utils'
	import { Spinner } from '$lib/components/ui/spinner'
	import { transformToOptions } from './utils'
	import { getCategoriesPaged } from '$lib/utils/client'
	import { parseCategoryId, parseCategoryTitle } from '$lib/utils/value-object'
	import { resolve } from '$app/paths'
	import { PUBLIC_APP_NAME } from '$env/static/public'

	let { data } = $props()

	const form = superForm(
		untrack(() => data.form),
		{ validators: valibot(vCreateThreadRequestBody) }
	)

	const { form: formData, enhance } = form
	const cancelHref = $derived.by(() => {
		const categoryId = parseCategoryId($formData.categoryId)
		return categoryId
			? resolve('/(app)/categories/[categoryId=CategoryId]', { categoryId })
			: resolve('/')
	})

	let open = $state(false)

	function closeAndFocusTrigger(triggerId: string) {
		open = false
		tick().then(() => {
			document.getElementById(triggerId)?.focus()
		})
	}

	const triggerId = useId()

	let options = $state(untrack(() => data.options))

	let loading = $state(false)
	let currentAbort: AbortController | null = null

	const fetchOptions = async (query: string) => {
		if (currentAbort) {
			currentAbort.abort()
			currentAbort = null
		}

		query = query.trim()

		const title = parseCategoryTitle(query)

		if (title === undefined) {
			loading = false
			return
		}

		loading = true
		const controller = new AbortController()
		currentAbort = controller

		try {
			const categories = (
				await getCategoriesPaged<true>({
					query: { title },
					signal: controller.signal
				})
			).data

			if (currentAbort !== controller) return
			options = transformToOptions(categories)
		} catch (error: unknown) {
			if (error instanceof Error && error.name === 'AbortError') {
				return
			}

			if (currentAbort === controller) {
				console.error('Ошибка при поиске:', error)
				options = []
			}
		} finally {
			if (currentAbort === controller) {
				currentAbort = null
				loading = false
			}
		}
	}

	let searchInputValue = $state('')

	$effect(() => {
		const query = searchInputValue
		const timeout = setTimeout(() => void fetchOptions(query), 300)

		return () => {
			clearTimeout(timeout)
			currentAbort?.abort()
		}
	})

	let selected = $derived(options.find((f) => f.key === $formData.categoryId)?.value)
</script>

<svelte:head>
	<title>Создание темы — {PUBLIC_APP_NAME}</title>
</svelte:head>

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
						<Popover.Content class="w-lg p-0">
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
												<Spinner />
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
			</Card.Content>
			<Card.Footer class="flex justify-between">
				<Button href={cancelHref} variant="outline">Отмена</Button>
				<Form.Button>Создать</Form.Button>
			</Card.Footer>
		</Card.Root>
	</form>
</div>
