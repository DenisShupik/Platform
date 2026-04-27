<script lang="ts">
	import * as Avatar from '$lib/components/ui/avatar'
	import { PUBLIC_AVATAR_URL } from '$env/static/public'
	import { formatTimestamp } from '$lib/utils/formatTimestamp'
	import IconClockFilled from '~icons/tabler/clock-filled'
	import type { PostDto, UserDto, Index } from '$lib/utils/client'
	import type { Snippet } from 'svelte'

	let {
		post,
		index,
		author,
		children
	}: { post: PostDto; index: Index; author: UserDto; children: Snippet<[]> | undefined } = $props()
</script>

<article
	id={'post-' + post.postId}
	class="grid w-full grid-flow-row overflow-hidden bg-muted/40 sm:grid-cols-[10em_auto] sm:rounded-lg sm:border sm:bg-inherit"
>
	<div
		class="grid w-full auto-cols-max grid-flow-col items-center gap-x-1 border-r p-2 sm:grid-flow-row sm:items-start sm:gap-x-0"
	>
		<Avatar.Root class="size-8 justify-self-center sm:size-16">
			<Avatar.Image src="{PUBLIC_AVATAR_URL}/{post.createdBy}" alt="@shadcn" />
			<Avatar.Fallback>CN</Avatar.Fallback>
		</Avatar.Root>
		<div class="justify-self-center text-sm font-semibold">{author.username}</div>
		<time class="flex items-center gap-x-1 text-xs text-muted-foreground sm:mt-2"
			><IconClockFilled class="size-3" />{formatTimestamp(author.createdAt)}</time
		>
	</div>
	<div>
		<header class="flex h-9 w-full items-center gap-1 bg-muted/40 px-2 py-0 text-base">
			<time class="flex-1 text-muted-foreground">{formatTimestamp(post.createdAt)}</time>
			{@render children?.()}
			<span class="text-muted-foreground">#{index}</span>
		</header>
		<div class="p-2">{post.content}</div>
	</div>
</article>
