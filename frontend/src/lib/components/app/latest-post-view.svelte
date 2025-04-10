<script lang="ts">
	import { formatTimestamp } from '$lib/utils/formatTimestamp'
	import * as Avatar from '$lib/components/ui/avatar'
	import type { PostDto, UserDto } from '$lib/utils/client'
	import { PUBLIC_AVATAR_URL } from '$env/static/public'

	let { post, author }: { post: PostDto; author: UserDto } = $props()
</script>

<div class="grid w-48 grid-cols-[auto_3em] gap-x-1">
	<div class="grid w-full self-center text-right text-sm font-medium">
		<div>{author.username}</div>
		<time class="text-muted-foreground line-clamp-1 text-xs"
			>{post != null ? formatTimestamp(post.created) : null}</time
		>
	</div>
	<Avatar.Root class="h-full w-full p-2">
		<Avatar.Image src="{PUBLIC_AVATAR_URL}/{author.userId}" alt="@shadcn" />
		<Avatar.Fallback>{author.username}</Avatar.Fallback>
	</Avatar.Root>
</div>
