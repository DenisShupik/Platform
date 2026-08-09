<script lang="ts">
	import { resolve } from '$app/paths'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import { CreateFormCard, RemoteCombobox } from '$lib/components/app'
	import * as Form from '$lib/components/ui/form'
	import { Input } from '$lib/components/ui/input'
	import { getForumsPaged } from '$lib/utils/client'
	import { vCreateCategoryRequestBody } from '$lib/utils/client/valibot.gen'
	import { parseForumId, parseForumTitle } from '$lib/utils/value-object'
	import { untrack } from 'svelte'
	import { fromAction } from 'svelte/attachments'
	import { superForm } from 'sveltekit-superforms'
	import { valibot } from 'sveltekit-superforms/adapters'
	import { transformToOptions } from './utils'

	let { data } = $props()

	// TODO: Verify that the user can create categories.

	const form = superForm(
		untrack(() => data.form),
		{ validators: valibot(vCreateCategoryRequestBody) }
	)

	const { form: formData, enhance } = form
	const enhanceAttachment = fromAction(enhance)
	const cancelHref = $derived.by(() => {
		const forumId = parseForumId($formData.forumId)
		return forumId ? resolve('/(app)/forums/[forumId=ForumId]', { forumId }) : resolve('/')
	})

	async function loadForums(query: string, signal: AbortSignal) {
		const title = parseForumTitle(query)
		if (title === undefined) return []

		const forums = (await getForumsPaged<true>({ query: { title }, signal })).data
		return transformToOptions(forums)
	}
</script>

<svelte:head>
	<title>Create category — {PUBLIC_APP_NAME}</title>
</svelte:head>

<div class="flex flex-1 items-center justify-center">
	<form method="POST" {@attach enhanceAttachment} class="w-full md:max-w-xl">
		<CreateFormCard
			title="Create category"
			description="Fill out the form to create a new category."
			{cancelHref}
		>
			<Form.Field {form} name="forumId" class="flex flex-col">
				<RemoteCombobox
					bind:value={$formData.forumId}
					label="Forum"
					placeholder="Select a forum…"
					searchPlaceholder="Search forums…"
					emptyText="No forums found"
					initialOptions={data.options}
					loadOptions={loadForums}
				/>
				<Form.FieldErrors />
			</Form.Field>
			<Form.Field {form} name="title">
				<Form.Control>
					{#snippet children({ props })}
						<Form.Label>Category title</Form.Label>
						<Input {...props} bind:value={$formData.title} />
					{/snippet}
				</Form.Control>
				<Form.FieldErrors />
			</Form.Field>
		</CreateFormCard>
	</form>
</div>
