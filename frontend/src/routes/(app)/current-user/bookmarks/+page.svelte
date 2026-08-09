<script lang="ts">
	import { invalidateAll } from '$app/navigation'
	import { resolve } from '$app/paths'
	import { Paginator, PostBookmarkButton, PostMarkdown } from '$lib/components/app'
	import { Button } from '$lib/components/ui/button'
	import * as Card from '$lib/components/ui/card'
	import * as Empty from '$lib/components/ui/empty'
	import { formatTimestamp } from '$lib/utils/format'
	import BookmarkIcon from '@lucide/svelte/icons/bookmark'
	import ExternalLinkIcon from '@lucide/svelte/icons/external-link'
	import type { PageProps } from './$types'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import * as m from '$lib/paraglide/messages'

	let { data }: PageProps = $props()

	async function handleBookmarkChange(isBookmarked: boolean) {
		if (!isBookmarked) await invalidateAll()
	}
</script>

<svelte:head>
	<title>{m.bookmarks()} — {PUBLIC_APP_NAME}</title>
</svelte:head>

<section class="flex flex-col gap-4">
	<div class="flex items-center gap-2">
		<BookmarkIcon class="text-muted-foreground" />
		<h1 class="text-xl font-bold sm:text-2xl">{m.bookmarks()}</h1>
	</div>

	{#if data.bookmarksData}
		<Paginator
			currentPage={data.currentPage}
			perPage={data.perPage}
			totalCount={data.bookmarkedPostsCount}
		/>

		<div class="flex flex-col gap-4">
			{#each data.bookmarksData.bookmarkedPosts as post (post.postId)}
				{@const thread = data.bookmarksData.threads.get(post.threadId)}
				<Card.Root size="sm">
					<Card.Header>
						<Card.Description>{m.post_message_in_thread()}</Card.Description>
						<Card.Title class="truncate">
							<a
								href={resolve(
									`/(app)/threads/[threadId=ThreadId]?post=${post.postId}#post-${post.postId}`,
									{ threadId: post.threadId }
								)}
								class="hover:underline"
							>
								{thread?.title ?? m.thread()}
							</a>
						</Card.Title>
						<Card.Action>
							<PostBookmarkButton
								postId={post.postId}
								initialIsBookmarked={true}
								onBookmarkChange={handleBookmarkChange}
							/>
						</Card.Action>
					</Card.Header>
					<Card.Content>
						<PostMarkdown html={post.renderedContent} />
					</Card.Content>
					<Card.Footer class="justify-between gap-4 border-t">
						<div class="min-w-0 truncate text-muted-foreground">
							{data.bookmarksData.users.get(post.createdBy)?.username ?? m.user()}
							<span aria-hidden="true"> · </span>
							<time datetime={post.createdAt.toISOString()}>{formatTimestamp(post.createdAt)}</time>
						</div>
						<Button
							href={resolve(
								`/(app)/threads/[threadId=ThreadId]?post=${post.postId}#post-${post.postId}`,
								{ threadId: post.threadId }
							)}
							variant="outline"
							size="sm"
						>
							{m.post_open_message()}
							<ExternalLinkIcon data-icon="inline-end" />
						</Button>
					</Card.Footer>
				</Card.Root>
			{/each}
		</div>
	{:else}
		<Empty.Root>
			<Empty.Header>
				<Empty.Media variant="icon"><BookmarkIcon /></Empty.Media>
				<Empty.Title>{m.bookmarks_none()}</Empty.Title>
				<Empty.Description>{m.bookmarks_empty_description()}</Empty.Description>
			</Empty.Header>
		</Empty.Root>
	{/if}
</section>
