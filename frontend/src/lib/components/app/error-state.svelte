<script lang="ts">
	import { resolve } from '$app/paths'
	import CircleAlertIcon from '@lucide/svelte/icons/circle-alert'
	import { Button } from '$lib/components/ui/button'

	let {
		title,
		description,
		status
	}: {
		title: string
		description?: string
		status?: number
	} = $props()
</script>

<section class="flex min-h-80 items-center justify-center py-12" aria-labelledby="error-title">
	<div class="flex max-w-md flex-col items-center gap-5 text-center">
		<div
			class="flex size-12 items-center justify-center rounded-full bg-destructive/10 text-destructive"
		>
			<CircleAlertIcon class="size-6" aria-hidden="true" />
		</div>
		<div class="space-y-2">
			{#if status}<p class="text-sm font-medium text-muted-foreground">Error {status}</p>{/if}
			<h1 id="error-title" class="text-2xl font-bold text-balance">{title}</h1>
			{#if description}<p class="text-sm text-pretty text-muted-foreground">{description}</p>{/if}
		</div>
		<Button href={resolve('/')} variant="outline">Back to forums</Button>
	</div>
</section>
