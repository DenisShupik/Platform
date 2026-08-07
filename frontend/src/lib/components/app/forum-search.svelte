<script lang="ts">
	import { resolve } from '$app/paths'
	import * as InputGroup from '$lib/components/ui/input-group'
	import SearchIcon from '@lucide/svelte/icons/search'

	const minTermLength = 2
	const maxTermLength = 100
</script>

<form
	action={resolve('/(app)/search')}
	method="GET"
	class="hidden min-w-0 flex-1 sm:block sm:flex-none"
	role="search"
>
	<label for="forum-search" class="sr-only">Search</label>
	<InputGroup.Root class="w-full sm:w-75 md:w-50 lg:w-75">
		<InputGroup.Input
			id="forum-search"
			name="q"
			type="search"
			placeholder="Search…"
			minlength={minTermLength}
			maxlength={maxTermLength}
			required
			autocomplete="off"
		/>
		<InputGroup.Addon align="inline-end">
			<InputGroup.Button type="submit" size="icon-sm" aria-label="Search">
				<SearchIcon data-icon="inline-start" />
			</InputGroup.Button>
		</InputGroup.Addon>
	</InputGroup.Root>
</form>
