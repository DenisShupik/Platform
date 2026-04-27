<script lang="ts">
	import type { Snippet } from 'svelte'
	import type { HTMLAttributes } from 'svelte/elements'
	import { cn } from '$lib/utils'

	let {
		class: className,
		children,
		...restProps
	}: HTMLAttributes<HTMLSpanElement> & { children?: Snippet } = $props()
</script>

<span class={cn('sr-only sm:not-sr-only sm:inline', className)} {...restProps}>
	{@render children?.()}
</span>
