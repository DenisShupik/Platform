<script lang="ts">
	import { cubicInOut } from 'svelte/easing'
	import { crossfade } from 'svelte/transition'
	import { cn } from '$lib/utils.js'
	import { Button } from '$lib/components/ui/button'
	import { resolve } from '$app/paths'
	import { page } from '$app/state'
	import type { Pathname } from '$app/types'

	type SidebarItem = { href: Pathname; title: string }

	let { items, class: className }: { items: SidebarItem[]; class?: string | null } = $props()

	const [send, receive] = crossfade({
		duration: 250,
		easing: cubicInOut
	})
</script>

<nav class={cn('flex gap-2 lg:flex-col lg:gap-1', className)}>
	{#each items as item (item.href)}
		{@const isActive = page.url.pathname === resolve(item.href)}
		<Button
			variant="ghost"
			class={cn(!isActive && 'hover:underline', 'relative justify-start hover:bg-transparent')}
			data-sveltekit-noscroll
		>
			{#if isActive}
				<div
					class="absolute inset-0 rounded-md bg-muted"
					in:send={{ key: 'active-sidebar-tab' }}
					out:receive={{ key: 'active-sidebar-tab' }}
				></div>
			{/if}
			<a href={resolve(item.href)} class="relative">{item.title}</a>
		</Button>
	{/each}
</nav>
