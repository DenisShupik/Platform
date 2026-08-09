<script lang="ts">
	import * as Form from '$lib/components/ui/form'
	import { Input } from '$lib/components/ui/input'
	import { superForm } from 'sveltekit-superforms'
	import { vCreateForumRequestBody } from '$lib/utils/client/valibot.gen'
	import { valibot } from 'sveltekit-superforms/adapters'
	import { untrack } from 'svelte'
	import { fromAction } from 'svelte/attachments'
	import { resolve } from '$app/paths'
	import * as m from '$lib/paraglide/messages'
	import { getLocale } from '$lib/paraglide/runtime'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import { CreateFormCard } from '$lib/components/app'
	let { data } = $props()

	const form = superForm(
		untrack(() => data.form),
		{ validators: valibot(vCreateForumRequestBody, { config: { lang: getLocale() } }) }
	)

	const { form: formData, enhance } = form
	const enhanceAttachment = fromAction(enhance)
</script>

<svelte:head>
	<title>{m.forum_create()} — {PUBLIC_APP_NAME}</title>
</svelte:head>

<div class="flex flex-1 items-center justify-center">
	<form method="POST" {@attach enhanceAttachment} class="w-full md:max-w-xl">
		<CreateFormCard
			title={m.forum_create()}
			description={m.forum_create_description()}
			cancelHref={resolve('/')}
		>
			<Form.Field {form} name="title">
				<Form.Control>
					{#snippet children({ props })}
						<Form.Label>{m.forum_title()}</Form.Label>
						<Input {...props} bind:value={$formData.title} />
					{/snippet}
				</Form.Control>
				<Form.FieldErrors />
			</Form.Field>
		</CreateFormCard>
	</form>
</div>
