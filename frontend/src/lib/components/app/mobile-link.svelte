<script lang="ts">
	import { page } from '$app/state'
	import { cn } from '$lib/utils.js'
	import type { Snippet } from 'svelte'

	let {
		children,
		href,
		open = $bindable(),
		class: className,
		...restProps
	}: {
		children: Snippet
		href: string
		open?: boolean
		class?: string | null
	} = $props()
</script>

<a
	{href}
	class={cn(page.url.pathname === href ? 'text-foreground' : 'text-foreground/60', className)}
	onclick={() => (open = false)}
	{...restProps}
>
	{@render children()}
</a>
