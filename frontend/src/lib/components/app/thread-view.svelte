<script lang="ts">
	import { formatTimestamp } from '$lib/utils/formatTimestamp'
	import * as Avatar from '$lib/components/ui/avatar'
	import { PostStat, LatestPostView } from '$lib/components/app'
	import { PUBLIC_AVATAR_URL } from '$env/static/public'
	import { IconClockFilled } from '@tabler/icons-svelte'
	import { type PostDto, type ThreadDto, type UserDto, type UserId } from '$lib/utils/client'

	let {
		thread,
		postCount,
		latestPost,
		users
	}: { thread: ThreadDto; postCount: bigint; latestPost: PostDto; users: Map<UserId, UserDto> } =
		$props()

	const categoryCreator = users.get(thread.createdBy)
	const latestPostAuthor = users.get(latestPost.createdBy)
</script>

<tr class="border">
	<td class="pl-4">
		<Avatar.Root class="h-full w-full p-2">
			<Avatar.Image src="{PUBLIC_AVATAR_URL}/{categoryCreator.userId}" alt="@shadcn" />
			<Avatar.Fallback>{categoryCreator.username}</Avatar.Fallback>
		</Avatar.Root>
	</td>
	<td class="border border-x-0 pl-2">
		<a href={`/threads/${thread.threadId}`} class="font-semibold leading-none tracking-tight"
			>{thread.title}
		</a>
		<p class="text-muted-foreground flex items-center gap-x-1 text-sm">
			<span>{categoryCreator.username}</span><IconClockFilled class="inline size-3" /><time
				>{formatTimestamp(thread.createdAt)}</time
			>
		</p>
	</td>
	<td class="hidden border md:table-cell"><PostStat count={postCount} class="mx-auto" /></td>
	<td class="hidden border border-r-0 text-right md:table-cell">
		<LatestPostView post={latestPost} author={latestPostAuthor} />
	</td>
</tr>
