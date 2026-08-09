<script lang="ts">
	import type { WithoutChildren } from 'bits-ui'
	import type { HTMLAttributes } from 'svelte/elements'
	import { cn } from '$lib/utils'
	import { formatCountUnit, formatNumber } from '$lib/utils/format'
	import type { Count } from '$lib/utils/client'

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
	{#if count !== undefined}
		<span>{formatNumber(count)}</span>
		<p class="text-xs font-normal text-muted-foreground">
			{formatCountUnit(count, 'thread')}
		</p>
	{/if}
</div>
