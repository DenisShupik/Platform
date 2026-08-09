<script lang="ts">
	import { cn } from '$lib/utils'
	import type { Snippet } from 'svelte'
	import type { HTMLAttributes } from 'svelte/elements'

	let {
		children,
		class: className,
		...restProps
	}: HTMLAttributes<HTMLDivElement> & { children: Snippet } = $props()
</script>

<div
	class={cn('mx-auto w-full max-w-(--breakpoint-2xl) px-4 sm:px-6 lg:px-8', className)}
	{...restProps}
>
	{@render children()}
</div>
