<script lang="ts">
	import { ForumView } from '$lib/components/app'
	import type { PageProps } from './$types'
	let { data }: PageProps = $props()
	console.log(data.forums.items)
</script>

<main class="flex max-w-3xl flex-col items-center justify-center gap-y-4 pt-8 sm:container">
	<div class="mt-2 space-y-4">
		{#each data.forums.items as forum}
			<ForumView {forum} />
		{/each}
	</div>
</main>
