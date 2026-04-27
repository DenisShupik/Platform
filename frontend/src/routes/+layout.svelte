<script lang="ts">
	import './layout.css'
	import { AppHeader } from '$lib/components/app'
	import { ModeWatcher } from 'mode-watcher'
	import type { LayoutProps } from './$types'
	import favicon from '$lib/assets/favicon.png'

	let { children }: LayoutProps = $props()
</script>

<svelte:head><link rel="icon" href={favicon} /></svelte:head>
<ModeWatcher />
<div class="relative flex min-h-screen flex-col bg-background">
	<AppHeader />
	{@render children?.()}
</div>
