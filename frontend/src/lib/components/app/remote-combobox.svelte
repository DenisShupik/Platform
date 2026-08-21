<script lang="ts" generics="TKey extends string">
	import CheckIcon from '@lucide/svelte/icons/check'
	import ChevronsUpDownIcon from '@lucide/svelte/icons/chevrons-up-down'
	import { useId } from 'bits-ui'
	import { tick, untrack } from 'svelte'
	import type { Attachment } from 'svelte/attachments'
	import { buttonVariants } from '$lib/components/ui/button'
	import * as Command from '$lib/components/ui/command'
	import * as Form from '$lib/components/ui/form'
	import * as Field from '$lib/components/ui/field'
	import * as Popover from '$lib/components/ui/popover'
	import { Spinner } from '$lib/components/ui/spinner'
	import { cn } from '$lib/utils'
	import * as m from '$lib/paraglide/messages'

	type Option = {
		key: TKey
		value: { title: string }
	}

	let {
		value = $bindable(),
		label,
		placeholder,
		searchPlaceholder,
		emptyText,
		standalone = false,
		initialOptions,
		loadOptions,
		onValueChange
	}: {
		value?: TKey
		label: string
		placeholder: string
		searchPlaceholder: string
		emptyText: string
		standalone?: boolean
		initialOptions: Option[]
		loadOptions: (query: string, signal: AbortSignal) => Promise<Option[]>
		onValueChange?: (value: TKey) => void
	} = $props()

	const triggerId = useId()
	let open = $state(false)
	let options = $state.raw<Option[]>(untrack(() => initialOptions))
	let selectedOption = $state.raw<Option | undefined>(
		untrack(() => initialOptions.find((option) => option.key === value))
	)
	let loading = $state(false)
	let loadError = $state(false)
	let currentAbort: AbortController | undefined
	let selected = $derived(
		options.find((option) => option.key === value)?.value ??
			(selectedOption?.key === value ? selectedOption?.value : undefined)
	)

	function closeAndFocusTrigger() {
		open = false
		void tick().then(() => document.getElementById(triggerId)?.focus())
	}

	async function fetchOptions(query: string) {
		currentAbort?.abort()

		const normalizedQuery = query.trim()
		loadError = false
		if (!normalizedQuery) {
			options = initialOptions
			loading = false
			return
		}

		const controller = new AbortController()
		currentAbort = controller
		loading = true

		try {
			const loadedOptions = await loadOptions(normalizedQuery, controller.signal)
			if (currentAbort === controller) options = loadedOptions
		} catch (error: unknown) {
			if (!(error instanceof Error && error.name === 'AbortError') && currentAbort === controller) {
				console.error('Failed to load combobox options:', error)
				options = []
				loadError = true
			}
		} finally {
			if (currentAbort === controller) {
				currentAbort = undefined
				loading = false
			}
		}
	}

	const searchOptions: Attachment<HTMLInputElement> = (input) => {
		let timeout: ReturnType<typeof setTimeout> | undefined
		const handleInput = () => {
			if (timeout) clearTimeout(timeout)
			timeout = setTimeout(() => void fetchOptions(input.value), 300)
		}

		input.addEventListener('input', handleInput)

		return () => {
			input.removeEventListener('input', handleInput)
			if (timeout) clearTimeout(timeout)
			currentAbort?.abort()
		}
	}
</script>

<Popover.Root bind:open>
	{#if standalone}
		<Field.Label for={triggerId}>{label}</Field.Label>
		<Popover.Trigger
			id={triggerId}
			type="button"
			class={cn(
				buttonVariants({ variant: 'outline' }),
				'w-full justify-between',
				!value && 'text-muted-foreground'
			)}
			role="combobox"
			aria-expanded={open}
		>
			{selected?.title ?? placeholder}
			<ChevronsUpDownIcon class="opacity-50" aria-hidden="true" />
		</Popover.Trigger>
	{:else}
		<Form.Control id={triggerId}>
			{#snippet children({ props })}
				<Form.Label>{label}</Form.Label>
				<Popover.Trigger
					class={cn(
						buttonVariants({ variant: 'outline' }),
						'w-full justify-between',
						!value && 'text-muted-foreground'
					)}
					role="combobox"
					aria-expanded={open}
					{...props}
				>
					{selected?.title ?? placeholder}
					<ChevronsUpDownIcon class="opacity-50" aria-hidden="true" />
				</Popover.Trigger>
				<input hidden {value} name={props.name} />
			{/snippet}
		</Form.Control>
	{/if}
	<Popover.Content class="w-(--bits-popover-anchor-width) max-w-[calc(100vw-2rem)] p-0">
		<Command.Root shouldFilter={false}>
			<Command.Input
				placeholder={searchPlaceholder}
				class="h-9"
				autocomplete="off"
				{@attach searchOptions}
			/>
			<Command.List>
				{#if !loading}
					<Command.Empty>
						{loadError ? m.error_options_load() : emptyText}
					</Command.Empty>
				{/if}
				{#if loading}
					<Command.Loading>
						<div
							class="flex items-center justify-center gap-2 pt-5 pb-4 text-sm"
							aria-live="polite"
						>
							<Spinner />
							<span>{m.common_loading()}</span>
						</div>
					</Command.Loading>
				{/if}
				<Command.Group>
					{#each options as option (option.key)}
						<Command.Item
							value={option.value.title}
							onSelect={() => {
								value = option.key
								selectedOption = option
								onValueChange?.(option.key)
								closeAndFocusTrigger()
							}}
						>
							{option.value.title}
							<CheckIcon
								class={cn('ml-auto', option.key !== value && 'text-transparent')}
								aria-hidden="true"
							/>
						</Command.Item>
					{/each}
				</Command.Group>
			</Command.List>
		</Command.Root>
	</Popover.Content>
</Popover.Root>
