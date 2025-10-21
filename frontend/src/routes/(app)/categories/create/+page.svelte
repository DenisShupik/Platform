<script lang="ts">
	import * as Form from '$lib/components/ui/form'
	import { Input } from '$lib/components/ui/input'
	import { superForm } from 'sveltekit-superforms'
	import { safeParse } from 'valibot'
	import { valibot } from 'sveltekit-superforms/adapters'
	import * as Card from '$lib/components/ui/card'
	import { vCreateCategoryRequestBody, vForumTitle } from '$lib/utils/client/valibot.gen'
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
	import {
		ReadPolicySelect,
		PostCreatePolicySelect,
		ThreadCreatePolicySelect
	} from '$lib/components/app/form-policy-select'
	import type { Option } from '../../../api/forums/utils'
	import { PolicyValue } from '$lib/utils/client'

	let { data } = $props()

	// TODO: сделать проверку кто, может создавать категории

	const form = superForm(data.form, { validators: valibot(vCreateCategoryRequestBody) })

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

		const result = safeParse(vForumTitle, query)

		if (!result.success) {
			loading = false
			return
		}

		loading = true
		const controller = new AbortController()
		currentAbort = controller

		try {
			const params = new URLSearchParams({ title: query })

			const response = await fetch(`/api/forums/?${params}`, {
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

	let selected = $derived(options.find((f) => f.key === $formData.forumId)?.value)
</script>

<div class="flex flex-1 items-center justify-center">
	<form method="POST" use:enhance class="w-full md:max-w-xl">
		<Card.Root class="border-0 md:border">
			<Card.Header>
				<Card.Title>Создание категории</Card.Title>
				<Card.Description>Заполните форму для создания новой категории</Card.Description>
			</Card.Header>
			<Card.Content>
				<Form.Field {form} name="forumId" class="flex flex-col">
					<Popover.Root bind:open>
						<Form.Control id={triggerId}>
							{#snippet children({ props })}
								<Form.Label>Форум</Form.Label>
								<Popover.Trigger
									class={cn(
										buttonVariants({ variant: 'outline' }),
										'w-full justify-between',
										!$formData.forumId && 'text-muted-foreground'
									)}
									role="combobox"
									{...props}
								>
									{selected?.title ?? 'Выберите форум...'}
									<ChevronsUpDown class="opacity-50" />
								</Popover.Trigger>
								<input hidden value={$formData.forumId} name={props.name} />
							{/snippet}
						</Form.Control>
						<Popover.Content class="w-[512px] p-0">
							<Command.Root shouldFilter={false}>
								<Command.Input
									autofocus
									placeholder="Введите название форума..."
									class="h-9"
									bind:value={searchInputValue}
								/>
								<Command.List>
									{#if !loading}
										<Command.Empty>Форумы не найдены</Command.Empty>
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
										{#each options as forum (forum.key)}
											<Command.Item
												value={forum.value.title}
												onSelect={() => {
													$formData.forumId = forum.key
													closeAndFocusTrigger(triggerId)
												}}
											>
												{forum.value.title}
												<Check
													class={cn(
														'ml-auto',
														forum.key !== $formData.forumId && 'text-transparent'
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
							<Form.Label>Category title</Form.Label>
							<Input {...props} bind:value={$formData.title} />
						{/snippet}
					</Form.Control>
					<Form.FieldErrors />
				</Form.Field>
				<ReadPolicySelect {form} inheritedValue={selected?.readPolicyValue ?? PolicyValue.ANY} />
				<ThreadCreatePolicySelect
					{form}
					inheritedValue={selected?.threadCreatePolicyValue ?? PolicyValue.ANY}
				/>
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
