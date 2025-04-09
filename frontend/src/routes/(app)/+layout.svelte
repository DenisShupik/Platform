<script lang="ts">
	import type { LayoutProps } from '../$types'

	let { children }: LayoutProps = $props()
</script>

<main class="py-8 sm:container">
	{@render children?.()}
</main>
