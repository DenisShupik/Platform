<script lang="ts">
	import { untrack } from 'svelte'
	import { SvelteMap } from 'svelte/reactivity'
	import type { AuthorizationScopeSelection } from '$lib/authorization-scope'
	import { withApiLocale } from '$lib/client/api-options'
	import * as Field from '$lib/components/ui/field'
	import * as ToggleGroup from '$lib/components/ui/toggle-group'
	import * as m from '$lib/paraglide/messages'
	import {
		AuthorizationScopeType,
		SearchResultType,
		getCategoriesPaged,
		getForumsPaged,
		search,
		type CategoryId,
		type ForumId,
		type ThreadId
	} from '$lib/utils/client'
	import {
		defaultPaginationLimit,
		parseCategoryTitle,
		parseForumTitle,
		parseSearchTerm
	} from '$lib/utils/value-object'
	import RemoteCombobox from './remote-combobox.svelte'

	const allScopes = [
		AuthorizationScopeType.PLATFORM,
		AuthorizationScopeType.FORUM,
		AuthorizationScopeType.CATEGORY,
		AuthorizationScopeType.THREAD
	] as const

	let {
		allowedScopes = allScopes,
		initialScopeType,
		onSelectionChange
	}: {
		allowedScopes?: readonly AuthorizationScopeType[]
		initialScopeType?: AuthorizationScopeType
		onSelectionChange: (selection: AuthorizationScopeSelection | undefined) => void
	} = $props()

	let scopeType = $state(
		untrack(() => initialScopeType ?? allowedScopes[0] ?? AuthorizationScopeType.PLATFORM)
	)
	let forumId = $state<ForumId>()
	let categoryId = $state<CategoryId>()
	let threadId = $state<ThreadId>()

	const categoryForums = new SvelteMap<CategoryId, ForumId>()
	const threadScopes = new SvelteMap<ThreadId, { forumId: ForumId; categoryId: CategoryId }>()

	const scopeOptions = [
		{ value: AuthorizationScopeType.PLATFORM, label: m.authorization_scope_platform() },
		{ value: AuthorizationScopeType.FORUM, label: m.authorization_scope_forum() },
		{ value: AuthorizationScopeType.CATEGORY, label: m.authorization_scope_category() },
		{ value: AuthorizationScopeType.THREAD, label: m.authorization_scope_thread() }
	].filter((option) => allowedScopes.includes(option.value))

	function selectScope(nextScopeType: AuthorizationScopeType) {
		scopeType = nextScopeType
		forumId = undefined
		categoryId = undefined
		threadId = undefined

		onSelectionChange(
			nextScopeType === AuthorizationScopeType.PLATFORM
				? {
						scopeType: AuthorizationScopeType.PLATFORM,
						forumId: null,
						categoryId: null,
						threadId: null
					}
				: undefined
		)
	}

	async function loadForums(query: string, signal: AbortSignal) {
		const title = parseForumTitle(query)
		if (!title) return []

		const response = await getForumsPaged<true>(
			withApiLocale({ query: { title, limit: defaultPaginationLimit }, signal, throwOnError: true })
		)
		return response.data.map((forum) => ({ key: forum.forumId, value: { title: forum.title } }))
	}

	async function loadCategories(query: string, signal: AbortSignal) {
		const title = parseCategoryTitle(query)
		if (!title) return []

		const response = await getCategoriesPaged<true>(
			withApiLocale({ query: { title, limit: defaultPaginationLimit }, signal, throwOnError: true })
		)
		for (const category of response.data) categoryForums.set(category.categoryId, category.forumId)

		return response.data.map((category) => ({
			key: category.categoryId,
			value: { title: category.title }
		}))
	}

	async function loadThreads(query: string, signal: AbortSignal) {
		const term = parseSearchTerm(query)
		if (!term) return []

		const response = await search<true>(
			withApiLocale({
				query: { term, type: SearchResultType.THREAD, limit: defaultPaginationLimit },
				signal,
				throwOnError: true
			})
		)

		const threads = response.data.items.filter(
			(result) =>
				result.threadId !== null && result.categoryId !== null && result.threadTitle !== null
		)
		for (const result of threads) {
			threadScopes.set(result.threadId!, {
				forumId: result.forumId,
				categoryId: result.categoryId!
			})
		}

		return threads.map((result) => ({
			key: result.threadId!,
			value: { title: result.threadTitle! }
		}))
	}

	function selectForum(selectedForumId: ForumId) {
		forumId = selectedForumId
		onSelectionChange({
			scopeType: AuthorizationScopeType.FORUM,
			forumId: selectedForumId,
			categoryId: null,
			threadId: null
		})
	}

	function selectCategory(selectedCategoryId: CategoryId) {
		categoryId = selectedCategoryId
		const selectedForumId = categoryForums.get(selectedCategoryId)
		if (!selectedForumId) {
			onSelectionChange(undefined)
			return
		}

		onSelectionChange({
			scopeType: AuthorizationScopeType.CATEGORY,
			forumId: selectedForumId,
			categoryId: selectedCategoryId,
			threadId: null
		})
	}

	function selectThread(selectedThreadId: ThreadId) {
		threadId = selectedThreadId
		const selectedScope = threadScopes.get(selectedThreadId)
		if (!selectedScope) {
			onSelectionChange(undefined)
			return
		}

		onSelectionChange({
			scopeType: AuthorizationScopeType.THREAD,
			forumId: selectedScope.forumId,
			categoryId: selectedScope.categoryId,
			threadId: selectedThreadId
		})
	}
</script>

<Field.FieldSet>
	<Field.FieldLegend variant="label">{m.authorization_scope()}</Field.FieldLegend>
	<ToggleGroup.Root
		type="single"
		value={scopeType}
		variant="outline"
		spacing={1}
		class="flex-wrap"
		aria-label={m.authorization_scope()}
	>
		{#each scopeOptions as option (option.value)}
			<ToggleGroup.Item value={option.value} onclick={() => selectScope(option.value)}>
				{option.label}
			</ToggleGroup.Item>
		{/each}
	</ToggleGroup.Root>
</Field.FieldSet>

{#if scopeType === AuthorizationScopeType.FORUM}
	<Field.Field>
		<RemoteCombobox
			bind:value={forumId}
			label={m.forum_title()}
			placeholder={m.forum_select()}
			searchPlaceholder={m.forum_search()}
			emptyText={m.forum_none()}
			standalone
			initialOptions={[]}
			loadOptions={loadForums}
			onValueChange={selectForum}
		/>
	</Field.Field>
{:else if scopeType === AuthorizationScopeType.CATEGORY}
	<Field.Field>
		<RemoteCombobox
			bind:value={categoryId}
			label={m.category_title()}
			placeholder={m.category_select()}
			searchPlaceholder={m.category_search()}
			emptyText={m.category_none()}
			standalone
			initialOptions={[]}
			loadOptions={loadCategories}
			onValueChange={selectCategory}
		/>
	</Field.Field>
{:else if scopeType === AuthorizationScopeType.THREAD}
	<Field.Field>
		<RemoteCombobox
			bind:value={threadId}
			label={m.thread_title()}
			placeholder={m.thread_select()}
			searchPlaceholder={m.thread_search()}
			emptyText={m.thread_none()}
			standalone
			initialOptions={[]}
			loadOptions={loadThreads}
			onValueChange={selectThread}
		/>
	</Field.Field>
{/if}
