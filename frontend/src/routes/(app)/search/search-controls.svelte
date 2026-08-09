<script lang="ts">
	import SearchIcon from '@lucide/svelte/icons/search'
	import * as Card from '$lib/components/ui/card'
	import * as Field from '$lib/components/ui/field'
	import * as InputGroup from '$lib/components/ui/input-group'
	import * as ToggleGroup from '$lib/components/ui/toggle-group'
	import { SearchResultType } from '$lib/utils/client'
	import type { SearchFilter, SearchSort } from './search-types'
	import * as m from '$lib/paraglide/messages'

	let {
		term = $bindable(),
		searchInput = $bindable(null),
		selectedType,
		selectedSort,
		minTermLength,
		maxTermLength,
		onSubmit,
		onTypeChange,
		onSortChange
	}: {
		term: string
		searchInput: HTMLInputElement | null
		selectedType: SearchFilter
		selectedSort: SearchSort
		minTermLength: number
		maxTermLength: number
		onSubmit: () => void | Promise<void>
		onTypeChange: (type: SearchFilter) => void | Promise<void>
		onSortChange: (sort: SearchSort) => void | Promise<void>
	} = $props()

	const typeFilters: { value: SearchFilter; label: string }[] = [
		{ value: 'all', label: m.search_all() },
		{ value: SearchResultType.FORUM, label: m.forums() },
		{ value: SearchResultType.CATEGORY, label: m.search_categories() },
		{ value: SearchResultType.THREAD, label: m.search_threads() },
		{ value: SearchResultType.POST, label: m.search_posts() }
	]
</script>

<Card.Root>
	<Card.Header>
		<Card.Title>{m.search()}</Card.Title>
		<Card.Description>{m.search_description()}</Card.Description>
	</Card.Header>
	<Card.Content class="flex flex-col gap-5">
		<form
			role="search"
			onsubmit={(event) => {
				event.preventDefault()
				void onSubmit()
			}}
		>
			<Field.FieldGroup>
				<Field.Field>
					<Field.FieldLabel for="search-query" class="sr-only">{m.search_query()}</Field.FieldLabel>
					<InputGroup.Root>
						<InputGroup.Input
							id="search-query"
							bind:ref={searchInput}
							bind:value={term}
							name="q"
							type="search"
							placeholder={m.search_query_placeholder()}
							minlength={minTermLength}
							maxlength={maxTermLength}
							required
							autocomplete="off"
						/>
						<InputGroup.Addon align="inline-end">
							<InputGroup.Button type="submit" size="sm">
								<SearchIcon data-icon="inline-start" aria-hidden="true" />
								{m.search()}
							</InputGroup.Button>
						</InputGroup.Addon>
					</InputGroup.Root>
				</Field.Field>
			</Field.FieldGroup>
		</form>

		<Field.FieldGroup class="flex-col gap-4 sm:flex-row sm:items-start">
			<Field.FieldSet>
				<Field.FieldLegend variant="label">{m.search_in()}</Field.FieldLegend>
				<ToggleGroup.Root
					type="single"
					value={selectedType}
					variant="outline"
					size="sm"
					spacing={1}
					aria-label={m.search_result_type()}
				>
					{#each typeFilters as filter (filter.value)}
						<ToggleGroup.Item value={filter.value} onclick={() => void onTypeChange(filter.value)}>
							{filter.label}
						</ToggleGroup.Item>
					{/each}
				</ToggleGroup.Root>
			</Field.FieldSet>

			<Field.FieldSet>
				<Field.FieldLegend variant="label">{m.search_sort_by()}</Field.FieldLegend>
				<ToggleGroup.Root
					type="single"
					value={selectedSort}
					variant="outline"
					size="sm"
					spacing={1}
					aria-label={m.search_sort_results()}
				>
					<ToggleGroup.Item value="relevance" onclick={() => void onSortChange('relevance')}>
						{m.search_most_relevant()}
					</ToggleGroup.Item>
					<ToggleGroup.Item value="newest" onclick={() => void onSortChange('newest')}>
						{m.search_newest_first()}
					</ToggleGroup.Item>
				</ToggleGroup.Root>
			</Field.FieldSet>
		</Field.FieldGroup>
	</Card.Content>
</Card.Root>
