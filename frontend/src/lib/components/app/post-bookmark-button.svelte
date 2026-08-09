<script lang="ts">
	import { withApiLocale } from '$lib/client/api-options'
	import { authClient } from '$lib/client'
	import { Spinner } from '$lib/components/ui/spinner'
	import { Toggle } from '$lib/components/ui/toggle'
	import { createPostBookmark, deletePostBookmark, type PostId } from '$lib/utils/client'
	import BookmarkIcon from '@lucide/svelte/icons/bookmark'
	import * as m from '$lib/paraglide/messages'

	let {
		postId,
		initialIsBookmarked,
		onBookmarkChange
	}: {
		postId: PostId
		initialIsBookmarked: boolean
		onBookmarkChange?: (isBookmarked: boolean) => void
	} = $props()

	let isBookmarkedOverride = $state<boolean | undefined>(undefined)
	let isBookmarked = $derived(isBookmarkedOverride ?? initialIsBookmarked)
	let isPending = $state(false)

	const session = authClient.useSession()

	async function updateBookmark(nextIsBookmarked: boolean) {
		if (isPending || nextIsBookmarked === isBookmarked) return

		isPending = true

		try {
			const result = nextIsBookmarked
				? await createPostBookmark<false>(withApiLocale({ path: { postId }, throwOnError: false }))
				: await deletePostBookmark<false>(withApiLocale({ path: { postId }, throwOnError: false }))

			if (result.error) {
				console.error('Bookmark action failed:', result.error)
				return
			}

			isBookmarkedOverride = nextIsBookmarked
			onBookmarkChange?.(nextIsBookmarked)
		} catch (error) {
			console.error('Bookmark action failed:', error)
		} finally {
			isPending = false
		}
	}
</script>

{#if $session.data}
	<Toggle
		pressed={isBookmarked}
		size="sm"
		aria-label={isBookmarked ? m.bookmark_remove() : m.bookmark_add()}
		disabled={isPending}
		onPressedChange={updateBookmark}
		class="size-8 p-0 data-[state=on]:*:[svg]:fill-primary data-[state=on]:*:[svg]:stroke-primary"
	>
		{#if isPending}
			<Spinner />
		{:else}
			<BookmarkIcon />
		{/if}
	</Toggle>
{/if}
