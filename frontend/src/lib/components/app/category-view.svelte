<script lang="ts">
	import { Separator } from '$lib/components/ui/separator'
	import { TopicStat, PostStat, LatestPostView } from '$lib/components/app'
	import type { CategoryDto, PostDto, UserDto, UserId } from '$lib/utils/client'
	import { resolve } from '$app/paths'

	let {
		category,
		threadCount,
		postCount,
		latestPost,
		users
	}: {
		category: CategoryDto
		threadCount: bigint
		postCount: bigint
		latestPost?: PostDto
		users: Map<UserId, UserDto>
	} = $props()
</script>

<div class="grid h-auto w-full grid-cols-[1fr_auto] items-center text-sm">
	<a
		href={resolve('/(app)/categories/[categoryId=CategoryId]', { categoryId: category.categoryId })}
		>{category.title}</a
	>
	<div class="grid grid-flow-col items-center">
		<TopicStat count={threadCount} class="hidden md:grid" />
		<Separator orientation="vertical" class="hidden md:inline" />
		<PostStat count={postCount} class="hidden md:grid" />
		<LatestPostView
			post={latestPost}
			author={latestPost?.createdBy ? users.get(latestPost?.createdBy) : undefined}
		/>
	</div>
</div>
