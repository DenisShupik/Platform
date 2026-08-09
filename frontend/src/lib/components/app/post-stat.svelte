<script lang="ts">
	import { cn } from '$lib/utils'
	import type { WithoutChildren } from 'bits-ui'
	import type { HTMLAttributes } from 'svelte/elements'
	import { formatCountUnit, formatNumber } from '$lib/utils/format'
	import type { Count } from '$lib/utils/client'

	let {
		count,
		class: className,
		...restProps
	}: { count?: Count } & WithoutChildren<HTMLAttributes<HTMLDivElement>> = $props()
</script>

<div class={cn('grid w-12 place-items-center text-sm font-medium', className)} {...restProps}>
	{#if count !== undefined}
		<span>{formatNumber(count)}</span>
		<p class="text-xs font-normal text-muted-foreground">
			{formatCountUnit(count, 'post')}
		</p>
	{/if}
</div>
