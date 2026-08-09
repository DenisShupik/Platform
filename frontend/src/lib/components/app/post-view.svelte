<script lang="ts">
	import * as Avatar from '$lib/components/ui/avatar'
	import { PUBLIC_AVATAR_URL } from '$env/static/public'
	import { formatDate, formatNumber, formatTimestamp } from '$lib/utils/format'
	import IconUserFilled from '~icons/tabler/user-filled'
	import PostMarkdown from './post-markdown.svelte'
	import type { UserDto, Index } from '$lib/utils/client'
	import type { RenderedPost } from '$lib/server/render-posts'
	import type { Snippet } from 'svelte'
	import * as m from '$lib/paraglide/messages'

	let {
		post,
		index,
		author,
		children
	}: {
		post: RenderedPost
		index: Index
		author: UserDto | undefined
		children: Snippet<[]> | undefined
	} = $props()

	const authorInitial = $derived(author?.username.at(0)?.toUpperCase() ?? '?')
</script>

<article
	id={'post-' + post.postId}
	class="grid w-full grid-flow-row overflow-hidden bg-muted/40 sm:grid-cols-[10rem_minmax(0,1fr)] sm:rounded-lg sm:border sm:bg-inherit"
>
	<div class="flex w-full items-center gap-2 border-r p-2 sm:flex-col sm:gap-1">
		<Avatar.Root class="size-8 shrink-0 sm:size-16">
			<Avatar.Image
				src="{PUBLIC_AVATAR_URL}/{post.createdBy}"
				alt={author ? `@${author.username}` : m.user_avatar()}
			/>
			<Avatar.Fallback>{authorInitial}</Avatar.Fallback>
		</Avatar.Root>
		<div class="text-sm font-semibold sm:text-center">
			{author?.username ?? m.user()}
		</div>
		<time
			datetime={(author?.createdAt ?? post.createdAt).toISOString()}
			title={m.post_member_since({ date: formatTimestamp(author?.createdAt ?? post.createdAt) })}
			aria-label={m.post_member_since({
				date: formatTimestamp(author?.createdAt ?? post.createdAt)
			})}
			class="ml-auto flex items-center gap-x-1 text-xs whitespace-nowrap text-muted-foreground sm:mt-1 sm:ml-0 sm:justify-center"
		>
			<IconUserFilled class="size-3 shrink-0" aria-hidden="true" />
			{formatDate(author?.createdAt ?? post.createdAt)}
		</time>
	</div>
	<div>
		<header class="flex h-9 w-full items-center gap-1 bg-muted/40 px-2 py-0 text-base">
			<time datetime={post.createdAt.toISOString()} class="flex-1 text-muted-foreground"
				>{formatTimestamp(post.createdAt)}</time
			>
			{@render children?.()}
			<span class="text-muted-foreground">#{formatNumber(index)}</span>
		</header>
		<div class="p-2"><PostMarkdown html={post.renderedContent} /></div>
	</div>
</article>
