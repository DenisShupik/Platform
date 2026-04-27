<script lang="ts">
	import { resolve } from '$app/paths'
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import type { ForumDto } from '$lib/utils/client'

	let {
		forum
	}: {
		forum: Pick<ForumDto, 'forumId' | 'title'>
	} = $props()
</script>

<Breadcrumb.Item>
	<a href={resolve('/(app)/forums/[forumId=ForumId]', { forumId: forum.forumId })}>{forum.title}</a>
</Breadcrumb.Item>
