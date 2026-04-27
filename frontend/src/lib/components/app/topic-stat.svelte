<script lang="ts">
	import type { WithoutChildren } from 'bits-ui'
	import type { HTMLAttributes } from 'svelte/elements'
	import { cn } from '$lib/utils'
	import { pluralize } from '$lib/utils/pluralize'
	import type { Count } from '$lib/utils/client'

	const forms: [string, string] = ['thread', 'threads']

	let {
		count,
		class: className,
		...restProps
	}: { count?: Count } & WithoutChildren<HTMLAttributes<HTMLDivElement>> = $props()
</script>

<div
	class={cn('grid w-14 place-items-center place-self-center text-sm font-medium', className)}
	{...restProps}
>
	{#if count}
		<h1>{count}</h1>
		<p class="text-muted-foreground text-xs font-normal">
			{pluralize(count, forms)}
		</p>
	{/if}
</div>
