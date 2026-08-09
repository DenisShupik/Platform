<script lang="ts">
	import { resolve } from '$app/paths'
	import { PUBLIC_AVATAR_URL } from '$env/static/public'
	import CircleAlertIcon from '@lucide/svelte/icons/circle-alert'
	import SearchIcon from '@lucide/svelte/icons/search'
	import SearchXIcon from '@lucide/svelte/icons/search-x'
	import * as Alert from '$lib/components/ui/alert'
	import * as Avatar from '$lib/components/ui/avatar'
	import { Badge } from '$lib/components/ui/badge'
	import { Button } from '$lib/components/ui/button'
	import * as Empty from '$lib/components/ui/empty'
	import * as Item from '$lib/components/ui/item'
	import { Skeleton } from '$lib/components/ui/skeleton'
	import { Spinner } from '$lib/components/ui/spinner'
	import {
		SearchResultType,
		type SearchCursor,
		type SearchResultDto,
		type UserDto,
		type UserId
	} from '$lib/utils/client'
	import { formatTimestamp } from '$lib/utils/format'
	import IconClockFilled from '~icons/tabler/clock-filled'
	import * as m from '$lib/paraglide/messages'
	import { formatNumber } from '$lib/utils/format'

	type SnippetPart = { text: string; highlighted: boolean }
	type ResultHref =
		| `/forums/${string}`
		| `/categories/${string}`
		| `/threads/${string}`
		| `/threads/${string}?${string}`
	let {
		results,
		users,
		searchedTerm,
		nextCursor,
		error,
		isLoading,
		isLoadingMore,
		onLoadMore
	}: {
		results: SearchResultDto[]
		users: ReadonlyMap<UserId, UserDto>
		searchedTerm?: string
		nextCursor?: SearchCursor
		error?: string
		isLoading: boolean
		isLoadingMore: boolean
		onLoadMore: () => void
	} = $props()

	function resultHref(result: SearchResultDto): ResultHref | undefined {
		switch (result.type) {
			case SearchResultType.FORUM:
				return result.forumId ? `/forums/${result.forumId}` : undefined
			case SearchResultType.CATEGORY:
				return result.categoryId ? `/categories/${result.categoryId}` : undefined
			case SearchResultType.THREAD:
				return result.threadId ? `/threads/${result.threadId}` : undefined
			case SearchResultType.POST:
				if (!result.threadId || !result.postId) return undefined
				return `/threads/${result.threadId}?post=${result.postId}#post-${result.postId}`
		}
	}

	function resultTypeLabel(type: SearchResultType) {
		switch (type) {
			case SearchResultType.FORUM:
				return m.forums()
			case SearchResultType.CATEGORY:
				return m.category()
			case SearchResultType.THREAD:
				return m.thread()
			case SearchResultType.POST:
				return m.stats_post_one()
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

<div aria-live="polite">
	{#if isLoading}
		<Item.Group aria-label={m.search_loading()}>
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
			<CircleAlertIcon aria-hidden="true" />
			<Alert.Title>{m.search_unavailable()}</Alert.Title>
			<Alert.Description>{error}</Alert.Description>
		</Alert.Root>
	{:else if !searchedTerm}
		<Empty.Root>
			<Empty.Header>
				<Empty.Media variant="icon"><SearchIcon aria-hidden="true" /></Empty.Media>
				<Empty.Title>{m.search_enter_query()}</Empty.Title>
				<Empty.Description>{m.search_results_here()}</Empty.Description>
			</Empty.Header>
		</Empty.Root>
	{:else if results.length === 0}
		<Empty.Root>
			<Empty.Header>
				<Empty.Media variant="icon"><SearchXIcon aria-hidden="true" /></Empty.Media>
				<Empty.Title>{m.search_no_results()}</Empty.Title>
				<Empty.Description>{m.search_no_results_for({ term: searchedTerm })}</Empty.Description>
			</Empty.Header>
		</Empty.Root>
	{:else}
		<section class="flex flex-col gap-4" aria-labelledby="search-results-title">
			<div class="flex flex-wrap items-center gap-2">
				<h2 id="search-results-title" class="text-lg font-semibold text-balance">
					{m.search_results_for({ term: searchedTerm })}
				</h2>
				<Badge variant="secondary" class="tabular-nums"
					>{m.search_showing({ count: formatNumber(results.length) })}</Badge
				>
			</div>

			<Item.Group aria-label={m.search_results()}>
				{#each results as result (resultKey(result))}
					{@const author = users.get(result.createdBy)}
					{@const href = resultHref(result)}
					{@const title = resultTitle(result)}
					{#snippet resultContent()}
						<Item.Media>
							<Avatar.Root class="size-8">
								<Avatar.Image
									src={`${PUBLIC_AVATAR_URL}/${result.createdBy}`}
									alt={author ? `@${author.username}` : m.user_avatar()}
									width="32"
									height="32"
									loading="lazy"
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
								<span>{author?.username ?? m.user()}</span>
								{#if result.type !== SearchResultType.FORUM}
									<span>· {result.forumTitle}</span>
								{/if}
								{#if result.categoryTitle}<span>· {result.categoryTitle}</span>{/if}
								{#if result.type === SearchResultType.POST && result.threadTitle}
									<span>· {result.threadTitle}</span>
								{/if}
								<time datetime={result.createdAt.toISOString()} class="flex items-center gap-x-1">
									<IconClockFilled class="size-3" aria-hidden="true" />
									{formatTimestamp(result.createdAt)}
								</time>
							</div>
						</Item.Content>
					{/snippet}
					<Item.Root variant="outline" size="sm">
						{#snippet child({ props })}
							{#if href}
								<a {...props} role="listitem" href={resolve(href)}>{@render resultContent()}</a>
							{:else}
								<div {...props} role="listitem">{@render resultContent()}</div>
							{/if}
						{/snippet}
					</Item.Root>
				{/each}
			</Item.Group>

			{#if nextCursor}
				<div class="flex justify-center">
					<Button variant="outline" disabled={isLoadingMore} onclick={onLoadMore}>
						{#if isLoadingMore}<Spinner data-icon="inline-start" />{/if}
						{m.search_show_more()}
					</Button>
				</div>
			{/if}
		</section>
	{/if}
</div>
