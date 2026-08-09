<script lang="ts">
	import { page } from '$app/state'
	import './layout.css'
	import { AppHeader } from '$lib/components/app'
	import * as Tooltip from '$lib/components/ui/tooltip'
	import { ModeWatcher } from 'mode-watcher'
	import type { LayoutProps } from './$types'
	import favicon from '$lib/assets/favicon.png'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import * as m from '$lib/paraglide/messages'

	let { children }: LayoutProps = $props()
	const canonicalUrl = $derived(new URL(page.url.pathname, page.url.origin).toString())
</script>

<svelte:head>
	<title>{PUBLIC_APP_NAME}</title>
	<meta name="description" content={m.forums()} />
	<link rel="canonical" href={canonicalUrl} />
	<link rel="icon" href={favicon} />
</svelte:head>

<ModeWatcher />

<Tooltip.Provider>
	<div class="relative flex min-h-screen flex-col bg-background">
		<a
			href="#main-content"
			class="sr-only z-60 rounded-md bg-background px-4 py-2 shadow-md focus:not-sr-only focus:absolute focus:top-4 focus:left-4"
			>{m.skip_to_content()}</a
		>

		<AppHeader />
		{@render children?.()}
	</div>
</Tooltip.Provider>
