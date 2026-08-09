<script lang="ts">
	import { AppContainer } from '$lib/components/app'
	import type { LayoutProps } from '../$types'

	let { children }: LayoutProps = $props()
</script>

<main id="main-content" tabindex="-1" class="py-8">
	<AppContainer>
		{@render children?.()}
	</AppContainer>
</main>
