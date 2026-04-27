<script lang="ts">
	import { page } from '$app/state'
	import { cn } from '$lib/utils.js'

	let {
		href,
		open = $bindable(),
		class: className,
		...restProps
	}: {
		href: string
		open?: boolean
		class?: string | null
	} = $props()
</script>

<a
	{href}
	class={cn(page.url.pathname === href ? 'text-foreground' : 'text-foreground/60', className)}
	on:click={() => (open = false)}
	{...restProps}
>
	<slot />
</a>
