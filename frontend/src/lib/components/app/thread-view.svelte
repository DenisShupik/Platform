<script lang="ts">
	import { formatTimestamp } from '$lib/utils/format'
	import * as Avatar from '$lib/components/ui/avatar'
	import { PostStat, LatestPostView } from '$lib/components/app'
	import { PUBLIC_AVATAR_URL } from '$env/static/public'
	import IconClockFilled from '~icons/tabler/clock-filled'
	import type { Count, PostDto, ThreadDto, UserDto, UserId } from '$lib/utils/client'
	import { resolve } from '$app/paths'

	let {
		thread,
		postCount,
		latestPost,
		users
	}: {
		thread: ThreadDto
		postCount: Count
		latestPost: PostDto | undefined
		users: Map<UserId, UserDto>
	} = $props()

	const categoryCreator = $derived(users.get(thread.createdBy))
	const latestPostAuthor = $derived(
		latestPost == null ? undefined : users.get(latestPost.createdBy)
	)
</script>

<tr class="border">
	<td class="pl-4">
		{#if categoryCreator}
			<Avatar.Root class="h-full w-full p-2">
				<Avatar.Image
					src="{PUBLIC_AVATAR_URL}/{categoryCreator.userId}"
					alt={`@${categoryCreator.username}`}
				/>
				<Avatar.Fallback>{categoryCreator.username}</Avatar.Fallback>
			</Avatar.Root>
		{/if}
	</td>
	<td class="border border-x-0 pl-2">
		<a
			href={resolve('/(app)/threads/[threadId=ThreadId]', { threadId: thread.threadId })}
			class="leading-none font-semibold tracking-tight"
			>{thread.title}
		</a>
		{#if categoryCreator}
			<p class="flex items-center gap-x-1 text-sm text-muted-foreground">
				<span>{categoryCreator.username}</span><IconClockFilled
					class="inline size-3"
					aria-hidden="true"
				/><time datetime={thread.createdAt.toISOString()}>{formatTimestamp(thread.createdAt)}</time>
			</p>
		{/if}
	</td>
	<td class="hidden border md:table-cell"><PostStat count={postCount} class="mx-auto" /></td>
	<td class="hidden border border-r-0 text-right md:table-cell">
		<LatestPostView post={latestPost} author={latestPostAuthor} />
	</td>
</tr>
