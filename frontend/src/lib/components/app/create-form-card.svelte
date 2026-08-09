<script lang="ts">
	import type { Snippet } from 'svelte'
	import { Button } from '$lib/components/ui/button'
	import * as Card from '$lib/components/ui/card'
	import * as Form from '$lib/components/ui/form'
	import * as m from '$lib/paraglide/messages'

	let {
		title,
		description,
		cancelHref,
		children
	}: {
		title: string
		description: string
		cancelHref: string
		children: Snippet
	} = $props()
</script>

<Card.Root class="border-0 md:border">
	<Card.Header>
		<Card.Title>{title}</Card.Title>
		<Card.Description>{description}</Card.Description>
	</Card.Header>
	<Card.Content>{@render children()}</Card.Content>
	<Card.Footer class="flex justify-between">
		<Button href={cancelHref} variant="outline">{m.common_cancel()}</Button>
		<Form.Button>{m.common_create()}</Form.Button>
	</Card.Footer>
</Card.Root>
