<script lang="ts">
	import { withApiLocale } from '$lib/client/api-options'
	import { afterNavigate, goto } from '$app/navigation'
	import { page } from '$app/state'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import {
		getUsersBulk,
		search,
		SearchQuerySortType,
		SearchResultType,
		type SearchCursor,
		type SearchResultDto,
		type UserDto,
		type UserId
	} from '$lib/utils/client'
	import { typedEntries } from '$lib/utils/typed-entries'
	import { defaultPaginationLimit, parseSearchTerm } from '$lib/utils/value-object'
	import { SvelteMap } from 'svelte/reactivity'
	import SearchControls from './search-controls.svelte'
	import SearchResults from './search-results.svelte'
	import type { SearchFilter, SearchSort } from './search-types'
	import * as m from '$lib/paraglide/messages'
	import { resolve } from '$app/paths'

	const minTermLength = 2
	const maxTermLength = 100
	const searchSortCriteria: Record<SearchSort, SearchQuerySortType> = {
		relevance: SearchQuerySortType.RELEVANCE_DESC,
		newest: SearchQuerySortType.NEWEST_DESC
	}

	let term = $state('')
	let searchedTerm = $state<string>()
	let results = $state.raw<SearchResultDto[]>([])
	const users = new SvelteMap<UserId, UserDto>()
	let nextCursor = $state<SearchCursor>()
	let error = $state<string>()
	let isLoading = $state(false)
	let isLoadingMore = $state(false)
	let selectedType = $state<SearchFilter>('all')
	let selectedSort = $state<SearchSort>('relevance')
	let searchInput = $state<HTMLInputElement | null>(null)
	let previousUrlKey: string | undefined
	let requestId = 0

	afterNavigate(() => {
		const urlTerm = page.url.searchParams.get('q')?.trim() ?? ''
		const urlType = parseType(page.url.searchParams.get('type'))
		const urlSort = parseSort(page.url.searchParams.get('sort'))
		const urlKey = `${urlTerm}\u0000${urlType ?? 'all'}\u0000${urlSort}`

		if (urlKey === previousUrlKey) return

		previousUrlKey = urlKey
		term = urlTerm
		selectedType = urlType ?? 'all'
		selectedSort = urlSort
		void loadResults(urlTerm)
	})

	function focusSearchInput() {
		searchInput?.focus()
	}

	function focusSearch(event: KeyboardEvent) {
		if (
			event.key !== '/' ||
			event.ctrlKey ||
			event.metaKey ||
			event.altKey ||
			isTextEntryTarget(event.target)
		) {
			return
		}

		event.preventDefault()
		focusSearchInput()
	}

	function parseType(value: string | null): SearchResultType | undefined {
		switch (value) {
			case SearchResultType.FORUM:
			case SearchResultType.CATEGORY:
			case SearchResultType.THREAD:
			case SearchResultType.POST:
				return value
			default:
				return undefined
		}
	}

	function parseSort(value: string | null): SearchSort {
		return value === 'newest' ? 'newest' : 'relevance'
	}

	function isTextEntryTarget(target: EventTarget | null) {
		return (
			target instanceof HTMLInputElement ||
			target instanceof HTMLTextAreaElement ||
			(target instanceof HTMLElement && target.isContentEditable)
		)
	}

	async function loadResults(searchTerm: string, append = false, cursor?: SearchCursor) {
		const currentRequestId = ++requestId
		searchedTerm = searchTerm || undefined
		error = undefined

		if (!append) {
			results = []
			users.clear()
			nextCursor = undefined
		}

		if (!searchTerm) return

		if (searchTerm.length < minTermLength || searchTerm.length > maxTermLength) {
			error = m.search_length_error({ min: minTermLength, max: maxTermLength })
			return
		}

		const parsedTerm = parseSearchTerm(searchTerm)
		if (parsedTerm === undefined) {
			error = m.search_empty_error()
			return
		}

		if (append) isLoadingMore = true
		else isLoading = true

		try {
			const response = await search<true>(
				withApiLocale({
					query: {
						term: parsedTerm,
						type: selectedType === 'all' ? undefined : selectedType,
						sort: searchSortCriteria[selectedSort],
						limit: defaultPaginationLimit,
						cursor
					},
					throwOnError: true
				})
			)

			if (currentRequestId !== requestId) return

			results = append ? [...results, ...response.data.items] : response.data.items
			nextCursor = response.data.nextCursor ?? undefined
			void loadAuthors(response.data.items, currentRequestId)
		} catch {
			if (currentRequestId === requestId) {
				error = m.search_failed()
			}
		} finally {
			if (currentRequestId === requestId) {
				isLoading = false
				isLoadingMore = false
			}
		}
	}

	async function loadAuthors(items: SearchResultDto[], currentRequestId: number) {
		const userIds = [...new Set(items.map((item) => item.createdBy))].filter(
			(userId) => !users.has(userId)
		)
		if (userIds.length === 0) return

		try {
			const response = await getUsersBulk<true>(
				withApiLocale({
					path: { userIds },
					throwOnError: true
				})
			)

			if (currentRequestId !== requestId) return

			for (const [userId, result] of typedEntries(response.data)) {
				if (result?.value) users.set(userId, result.value)
			}
		} catch {
			// Search results remain useful when author details are unavailable.
		}
	}

	async function submitSearch() {
		await navigate(term.trim(), selectedType, selectedSort, true)
	}

	async function updateType(type: SearchFilter) {
		selectedType = type
		await navigate(term.trim(), type, selectedSort)
	}

	async function updateSort(sort: SearchSort) {
		selectedSort = sort
		await navigate(term.trim(), selectedType, sort)
	}

	async function navigate(
		searchTerm: string,
		type: SearchFilter,
		sort: SearchSort,
		refreshCurrent = false
	) {
		const url = new URL(resolve('/(app)/search'), page.url)

		if (searchTerm) url.searchParams.set('q', searchTerm)
		else url.searchParams.delete('q')

		if (type === 'all') url.searchParams.delete('type')
		else url.searchParams.set('type', type)

		if (sort === 'relevance') url.searchParams.delete('sort')
		else url.searchParams.set('sort', sort)

		if (url.pathname === page.url.pathname && url.search === page.url.search) {
			if (refreshCurrent) await loadResults(searchTerm)
			return
		}

		const href = url.search ? (`/search${url.search}` as `/search?${string}`) : '/search'
		await goto(resolve(href))
	}

	function loadMore() {
		if (!nextCursor || isLoadingMore) return
		void loadResults(searchedTerm ?? '', true, nextCursor)
	}
</script>

<svelte:head>
	<title>{m.search()} — {PUBLIC_APP_NAME}</title>
</svelte:head>

<svelte:window onkeydown={focusSearch} />

<div class="mx-auto flex w-full max-w-5xl flex-col gap-6">
	<h1 class="sr-only">{m.search()}</h1>

	<SearchControls
		bind:term
		bind:searchInput
		{selectedType}
		{selectedSort}
		{minTermLength}
		{maxTermLength}
		onSubmit={submitSearch}
		onTypeChange={updateType}
		onSortChange={updateSort}
	/>

	<SearchResults
		{results}
		{users}
		{searchedTerm}
		{nextCursor}
		{error}
		{isLoading}
		{isLoadingMore}
		onLoadMore={loadMore}
	/>
</div>
