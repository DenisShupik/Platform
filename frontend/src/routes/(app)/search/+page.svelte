<script lang="ts">
	import { goto } from '$app/navigation'
	import { resolve } from '$app/paths'
	import { page } from '$app/state'
	import type { ResolvedPathname } from '$app/types'
	import { PUBLIC_AVATAR_URL } from '$env/static/public'
	import * as Alert from '$lib/components/ui/alert'
	import * as Avatar from '$lib/components/ui/avatar'
	import { Badge } from '$lib/components/ui/badge'
	import { Button } from '$lib/components/ui/button'
	import * as Card from '$lib/components/ui/card'
	import * as Empty from '$lib/components/ui/empty'
	import * as Field from '$lib/components/ui/field'
	import * as InputGroup from '$lib/components/ui/input-group'
	import * as Item from '$lib/components/ui/item'
	import { Skeleton } from '$lib/components/ui/skeleton'
	import { Spinner } from '$lib/components/ui/spinner'
	import * as ToggleGroup from '$lib/components/ui/toggle-group'
	import { formatTimestamp } from '$lib/utils/formatTimestamp'
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
	import { defaultPaginationLimit, parseSearchTerm } from '$lib/utils/value-object'
	import { typedEntries } from '$lib/utils/typed-entries'
	import CircleAlertIcon from '@lucide/svelte/icons/circle-alert'
	import SearchIcon from '@lucide/svelte/icons/search'
	import SearchXIcon from '@lucide/svelte/icons/search-x'
	import { SvelteMap } from 'svelte/reactivity'
	import IconClockFilled from '~icons/tabler/clock-filled'
	import { PUBLIC_APP_NAME } from '$env/static/public'

	const minTermLength = 2
	const maxTermLength = 100

	type SearchFilter = 'all' | SearchResultType
	type SearchSort = 'relevance' | 'newest'
	type SnippetPart = { text: string; highlighted: boolean }

	const typeFilters: { value: SearchFilter; label: string }[] = [
		{ value: 'all', label: 'Все' },
		{ value: SearchResultType.FORUM, label: 'Форумы' },
		{ value: SearchResultType.CATEGORY, label: 'Разделы' },
		{ value: SearchResultType.THREAD, label: 'Темы' },
		{ value: SearchResultType.POST, label: 'Сообщения' }
	]

	const searchSortCriteria: Record<SearchSort, SearchQuerySortType> = {
		relevance: SearchQuerySortType.RELEVANCE_DESC,
		newest: SearchQuerySortType.NEWEST_DESC
	}

	let term = $state('')
	let searchedTerm = $state<string>()
	let results = $state<SearchResultDto[]>([])
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

	function focusSearchInput() {
		searchInput?.focus()
	}

	$effect(() => {
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
			error = `Введите от ${minTermLength} до ${maxTermLength} символов.`
			return
		}
		const parsedTerm = parseSearchTerm(searchTerm)
		if (parsedTerm === undefined) {
			error = 'Введите непустой поисковый запрос.'
			return
		}

		if (append) isLoadingMore = true
		else isLoading = true

		try {
			const response = await search<true>({
				query: {
					term: parsedTerm,
					type: selectedType === 'all' ? undefined : selectedType,
					sort: searchSortCriteria[selectedSort],
					limit: defaultPaginationLimit,
					cursor
				},
				throwOnError: true
			})

			if (currentRequestId !== requestId) return

			results = append ? [...results, ...response.data.items] : response.data.items
			nextCursor = response.data.nextCursor ?? undefined
			void loadAuthors(response.data.items, currentRequestId)
		} catch {
			if (currentRequestId === requestId) {
				error = 'Не удалось выполнить поиск. Попробуйте ещё раз.'
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
			const response = await getUsersBulk<true>({
				path: { userIds },
				throwOnError: true
			})

			if (currentRequestId !== requestId) return

			for (const [userId, result] of typedEntries(response.data)) {
				if (result?.value) users.set(userId, result.value)
			}
		} catch {
			// Результат остаётся полезным и без карточек авторов.
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

		const destination = withSearch(resolve('/(app)/search'), url.search)
		const current = page.url.pathname + page.url.search
		if (destination === current) {
			if (refreshCurrent) await loadResults(searchTerm)
			return
		}

		await goto(destination)
	}

	function loadMore() {
		if (!nextCursor || isLoadingMore) return
		void loadResults(searchedTerm ?? '', true, nextCursor)
	}

	function resultHref(result: SearchResultDto): ResolvedPathname | undefined {
		switch (result.type) {
			case SearchResultType.FORUM:
				return result.forumId
					? resolve('/(app)/forums/[forumId=ForumId]', { forumId: result.forumId })
					: undefined
			case SearchResultType.CATEGORY:
				return result.categoryId
					? resolve('/(app)/categories/[categoryId=CategoryId]', { categoryId: result.categoryId })
					: undefined
			case SearchResultType.THREAD:
				return result.threadId
					? resolve('/(app)/threads/[threadId=ThreadId]', { threadId: result.threadId })
					: undefined
			case SearchResultType.POST:
				return result.threadId && result.postId
					? withSearch(
							resolve('/(app)/threads/[threadId=ThreadId]', { threadId: result.threadId }),
							`?post=${result.postId}#post-${result.postId}`
						)
					: undefined
		}
	}

	function withSearch(pathname: ResolvedPathname, search: string): ResolvedPathname {
		return `${pathname}${search}` as ResolvedPathname
	}

	function resultTypeLabel(type: SearchResultType) {
		switch (type) {
			case SearchResultType.FORUM:
				return 'Форум'
			case SearchResultType.CATEGORY:
				return 'Раздел'
			case SearchResultType.THREAD:
				return 'Тема'
			case SearchResultType.POST:
				return 'Сообщение'
		}
	}

	function resultKey(result: SearchResultDto) {
		return `${result.type}-${result.postId ?? result.threadId ?? result.categoryId ?? result.forumId}`
	}

	function resultTitle(result: SearchResultDto) {
		switch (result.type) {
			case SearchResultType.FORUM:
				return result.forumTitle
			case SearchResultType.CATEGORY:
				return result.categoryTitle ?? result.forumTitle
			case SearchResultType.THREAD:
			case SearchResultType.POST:
				return result.threadTitle ?? result.categoryTitle ?? result.forumTitle
		}
	}

	function snippetParts(snippet: string): SnippetPart[] {
		let highlighted = false
		const parts: SnippetPart[] = []

		for (const part of snippet.split(/(⟦|⟧)/)) {
			if (part === '⟦') {
				highlighted = true
				continue
			}
			if (part === '⟧') {
				highlighted = false
				continue
			}
			if (part) parts.push({ text: part, highlighted })
		}

		return parts
	}
</script>

<svelte:head>
	<title>Поиск — {PUBLIC_APP_NAME}</title>
</svelte:head>

<svelte:window onkeydown={focusSearch} />

<div class="mx-auto flex w-full max-w-5xl flex-col gap-6 px-4 sm:px-0">
	<h1 class="sr-only">Поиск</h1>

	<Card.Root>
		<Card.Header>
			<Card.Title>Поиск</Card.Title>
			<Card.Description>По форумам, разделам, темам и сообщениям.</Card.Description>
		</Card.Header>
		<Card.Content class="flex flex-col gap-5">
			<form
				role="search"
				onsubmit={(event) => {
					event.preventDefault()
					void submitSearch()
				}}
			>
				<Field.FieldGroup>
					<Field.Field>
						<Field.FieldLabel for="search-query" class="sr-only">Поисковый запрос</Field.FieldLabel>
						<InputGroup.Root>
							<InputGroup.Input
								id="search-query"
								bind:ref={searchInput}
								bind:value={term}
								name="q"
								type="search"
								placeholder="Введите запрос"
								minlength={minTermLength}
								maxlength={maxTermLength}
								required
								autocomplete="off"
								autofocus={!page.url.searchParams.has('q')}
							/>
							<InputGroup.Addon align="inline-end">
								<InputGroup.Button type="submit" size="sm">
									<SearchIcon data-icon="inline-start" />
									Найти
								</InputGroup.Button>
							</InputGroup.Addon>
						</InputGroup.Root>
					</Field.Field>
				</Field.FieldGroup>
			</form>

			<Field.FieldGroup class="flex-col gap-4 sm:flex-row sm:items-start">
				<Field.FieldSet>
					<Field.FieldLegend variant="label">Искать в</Field.FieldLegend>
					<ToggleGroup.Root
						type="single"
						value={selectedType}
						variant="outline"
						size="sm"
						spacing={1}
						aria-label="Тип результата"
					>
						{#each typeFilters as filter (filter.value)}
							<ToggleGroup.Item value={filter.value} onclick={() => void updateType(filter.value)}>
								{filter.label}
							</ToggleGroup.Item>
						{/each}
					</ToggleGroup.Root>
				</Field.FieldSet>

				<Field.FieldSet>
					<Field.FieldLegend variant="label">Сортировка</Field.FieldLegend>
					<ToggleGroup.Root
						type="single"
						value={selectedSort}
						variant="outline"
						size="sm"
						spacing={1}
						aria-label="Сортировка результатов"
					>
						<ToggleGroup.Item value="relevance" onclick={() => void updateSort('relevance')}>
							По релевантности
						</ToggleGroup.Item>
						<ToggleGroup.Item value="newest" onclick={() => void updateSort('newest')}>
							Сначала новые
						</ToggleGroup.Item>
					</ToggleGroup.Root>
				</Field.FieldSet>
			</Field.FieldGroup>
		</Card.Content>
	</Card.Root>

	{#if isLoading}
		<Item.Group aria-label="Загрузка результатов поиска">
			{#each [0, 1, 2, 3] as index (index)}
				<Item.Root variant="outline" size="sm" aria-hidden="true">
					<Item.Media><Skeleton class="size-8 rounded-full" /></Item.Media>
					<Item.Content>
						<Skeleton class="h-4 w-2/5" />
						<Skeleton class="h-3 w-4/5" />
						<Skeleton class="h-3 w-1/3" />
					</Item.Content>
				</Item.Root>
			{/each}
		</Item.Group>
	{:else if error}
		<Alert.Root variant="destructive">
			<CircleAlertIcon />
			<Alert.Title>Поиск недоступен</Alert.Title>
			<Alert.Description>{error}</Alert.Description>
		</Alert.Root>
	{:else if !searchedTerm}
		<Empty.Root>
			<Empty.Header>
				<Empty.Media variant="icon"><SearchIcon /></Empty.Media>
				<Empty.Title>Введите поисковый запрос</Empty.Title>
				<Empty.Description>Результаты появятся здесь.</Empty.Description>
			</Empty.Header>
		</Empty.Root>
	{:else if results.length === 0}
		<Empty.Root>
			<Empty.Header>
				<Empty.Media variant="icon"><SearchXIcon /></Empty.Media>
				<Empty.Title>Ничего не найдено</Empty.Title>
				<Empty.Description>По запросу «{searchedTerm}» результатов нет.</Empty.Description>
			</Empty.Header>
		</Empty.Root>
	{:else}
		<section class="flex flex-col gap-4" aria-labelledby="search-results-title">
			<div class="flex flex-wrap items-center gap-2">
				<h2 id="search-results-title" class="text-lg font-semibold">
					Результаты по запросу «{searchedTerm}»
				</h2>
				<Badge variant="secondary">Показано {results.length}</Badge>
			</div>

			<Item.Group aria-label="Результаты поиска">
				{#each results as result (resultKey(result))}
					{@const author = users.get(result.createdBy)}
					{@const href = resultHref(result)}
					{@const title = resultTitle(result)}
					<Item.Root variant="outline" size="sm">
						{#snippet child({ props })}
							<a {...props} role="listitem" {href}>
								<Item.Media>
									<Avatar.Root class="size-8">
										<Avatar.Image
											src={`${PUBLIC_AVATAR_URL}/${result.createdBy}`}
											alt={author ? `@${author.username}` : 'Аватар автора'}
										/>
										<Avatar.Fallback
											>{author?.username.slice(0, 1).toUpperCase() ?? '?'}</Avatar.Fallback
										>
									</Avatar.Root>
								</Item.Media>
								<Item.Content>
									<Item.Title class="w-full">
										<Badge variant="secondary">{resultTypeLabel(result.type)}</Badge>
										<span class="truncate">{title}</span>
									</Item.Title>

									{#if result.snippet}
										<Item.Description>
											{#each snippetParts(result.snippet) as part, index (`${part.text}-${index}`)}
												{#if part.highlighted}<mark>{part.text}</mark>{:else}{part.text}{/if}
											{/each}
										</Item.Description>
									{/if}

									<div class="flex flex-wrap items-center gap-x-1.5 text-xs text-muted-foreground">
										<span>{author?.username ?? 'Пользователь'}</span>
										{#if result.type !== SearchResultType.FORUM}
											<span>· {result.forumTitle}</span>
										{/if}
										{#if result.categoryTitle}<span>· {result.categoryTitle}</span>{/if}
										{#if result.type === SearchResultType.POST && result.threadTitle}
											<span>· {result.threadTitle}</span>
										{/if}
										<time
											datetime={result.createdAt.toISOString()}
											class="flex items-center gap-x-1"
										>
											<IconClockFilled class="size-3" />
											{formatTimestamp(result.createdAt)}
										</time>
									</div>
								</Item.Content>
							</a>
						{/snippet}
					</Item.Root>
				{/each}
			</Item.Group>

			{#if nextCursor}
				<div class="flex justify-center">
					<Button variant="outline" disabled={isLoadingMore} onclick={loadMore}>
						{#if isLoadingMore}<Spinner data-icon="inline-start" />{/if}
						Показать ещё
					</Button>
				</div>
			{/if}
		</section>
	{/if}
</div>
