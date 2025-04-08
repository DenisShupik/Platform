<script lang="ts">
	import { cn } from '$lib/utils'
	import type { WithoutChildren } from 'bits-ui'
	import type { HTMLAttributes } from 'svelte/elements'
	import { pluralize } from '$lib/utils/pluralize'

	const forms: [string, string] = ['post', 'posts']

	let {
		count,
		class: className,
		...restProps
	}: { count: bigint } & WithoutChildren<HTMLAttributes<HTMLDivElement>> = $props()
</script>

<div class={cn('grid w-12 place-items-center text-sm font-medium', className)} {...restProps}>
	<h1>{count}</h1>
	<p class="text-muted-foreground text-xs font-normal">
		{pluralize(count, forms)}
	</p>
</div>
