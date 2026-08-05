<script lang="ts">
	import { page } from '$app/state'
	import { resolve } from '$app/paths'
	import type { Pathname } from '$app/types'
	import { cn } from '$lib/utils.js'
	import type { Snippet } from 'svelte'

	let {
		children,
		href,
		onNavigate,
		class: className,
		...restProps
	}: {
		children: Snippet
		href: Pathname
		onNavigate?: () => void
		class?: string | null
	} = $props()
</script>

<a
	href={resolve(href)}
	class={cn(
		page.url.pathname === resolve(href) ? 'text-foreground' : 'text-foreground/60',
		className
	)}
	onclick={onNavigate}
	{...restProps}
>
	{@render children()}
</a>
