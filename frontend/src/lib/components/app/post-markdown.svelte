<script lang="ts">
	import { PreRendered } from 'carta-md'
	import { cn } from '$lib/utils'

	let { html, class: className }: { html: string; class?: string } = $props()
</script>

<div
	class={cn(
		'post-markdown prose max-w-none break-words prose-neutral dark:prose-invert',
		className
	)}
>
	<PreRendered {html} />
</div>
