<script lang="ts">
	import { Button } from '$lib/components/ui/button'
	import * as Dialog from '$lib/components/ui/dialog'
	import { Spinner } from '$lib/components/ui/spinner'
	import * as m from '$lib/paraglide/messages'

	let {
		open = $bindable(false),
		title,
		description,
		confirmLabel,
		busy = false,
		onConfirm
	}: {
		open?: boolean
		title: string
		description: string
		confirmLabel: string
		busy?: boolean
		onConfirm: () => void | Promise<void>
	} = $props()
</script>

<Dialog.Root bind:open>
	<Dialog.Content class="sm:max-w-md" showCloseButton={!busy}>
		<Dialog.Header>
			<Dialog.Title>{title}</Dialog.Title>
			<Dialog.Description>{description}</Dialog.Description>
		</Dialog.Header>
		<Dialog.Footer>
			<Button type="button" variant="outline" disabled={busy} onclick={() => (open = false)}>
				{m.common_cancel()}
			</Button>
			<Button type="button" variant="destructive" disabled={busy} onclick={onConfirm}>
				{#if busy}<Spinner data-icon="inline-start" />{/if}
				{confirmLabel}
			</Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
