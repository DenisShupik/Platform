<script lang="ts">
	import { goto } from '$app/navigation'
	import { resolve } from '$app/paths'
	import { page } from '$app/state'
	import * as InputGroup from '$lib/components/ui/input-group'
	import SearchIcon from '@lucide/svelte/icons/search'

	const minTermLength = 2
	const maxTermLength = 100

	let term = $state('')

	async function submitSearch() {
		const searchTerm = term.trim()
		if (searchTerm.length < minTermLength || searchTerm.length > maxTermLength) return

		const url = new URL(resolve('/(app)/search'), page.url)
		url.searchParams.set('q', searchTerm)
		await goto(url.pathname + url.search)
	}
</script>

<form
	class="min-w-0 flex-1 sm:flex-none"
	role="search"
	onsubmit={(event) => {
		event.preventDefault()
		void submitSearch()
	}}
>
	<label for="forum-search" class="sr-only">Поиск по форуму</label>
	<InputGroup.Root class="w-full sm:w-75 md:w-50 lg:w-75">
		<InputGroup.Input
			id="forum-search"
			bind:value={term}
			name="q"
			type="search"
			placeholder="Поиск..."
			minlength={minTermLength}
			maxlength={maxTermLength}
			required
			autocomplete="off"
		/>
		<InputGroup.Addon align="inline-end">
			<InputGroup.Button type="submit" size="icon-sm" aria-label="Найти">
				<SearchIcon data-icon="inline-start" />
			</InputGroup.Button>
		</InputGroup.Addon>
	</InputGroup.Root>
</form>
