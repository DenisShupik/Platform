<script lang="ts">
	import type { LayoutProps } from '../$types'

	let { children }: LayoutProps = $props()
</script>

<main class="px-0 py-8 sm:px-8">
	{@render children?.()}
</main>
