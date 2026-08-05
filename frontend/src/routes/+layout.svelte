<script lang="ts">
	import './layout.css'
	import { AppHeader } from '$lib/components/app'
	import { ModeWatcher } from 'mode-watcher'
	import type { LayoutProps } from './$types'
	import favicon from '$lib/assets/favicon.png'
	import { PUBLIC_APP_NAME } from '$env/static/public'

	let { children }: LayoutProps = $props()
</script>

<svelte:head>
	<title>{PUBLIC_APP_NAME}</title>
	<link rel="icon" href={favicon} />
</svelte:head>
<ModeWatcher />
<div class="relative flex min-h-screen flex-col bg-background">
	<a
		href="#main-content"
		class="sr-only z-60 rounded-md bg-background px-4 py-2 shadow-md focus:not-sr-only focus:absolute focus:top-4 focus:left-4"
	>
		Перейти к содержимому
	</a>
	<AppHeader />
	{@render children?.()}
</div>
