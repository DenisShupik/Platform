<script lang="ts">
	import { resolve } from '$app/paths'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import { CreateFormCard, RemoteCombobox } from '$lib/components/app'
	import * as Form from '$lib/components/ui/form'
	import { Input } from '$lib/components/ui/input'
	import { getCategoriesPaged } from '$lib/utils/client'
	import { vCreateThreadRequestBody } from '$lib/utils/client/valibot.gen'
	import { parseCategoryId, parseCategoryTitle } from '$lib/utils/value-object'
	import { untrack } from 'svelte'
	import { fromAction } from 'svelte/attachments'
	import { superForm } from 'sveltekit-superforms'
	import { valibot } from 'sveltekit-superforms/adapters'
	import { transformToOptions } from './utils'

	let { data } = $props()

	const form = superForm(
		untrack(() => data.form),
		{ validators: valibot(vCreateThreadRequestBody) }
	)

	const { form: formData, enhance } = form
	const enhanceAttachment = fromAction(enhance)
	const cancelHref = $derived.by(() => {
		const categoryId = parseCategoryId($formData.categoryId)
		return categoryId
			? resolve('/(app)/categories/[categoryId=CategoryId]', { categoryId })
			: resolve('/')
	})

	async function loadCategories(query: string, signal: AbortSignal) {
		const title = parseCategoryTitle(query)
		if (title === undefined) return []

		const categories = (await getCategoriesPaged<true>({ query: { title }, signal })).data
		return transformToOptions(categories)
	}
</script>

<svelte:head>
	<title>Create thread — {PUBLIC_APP_NAME}</title>
</svelte:head>

<div class="flex flex-1 items-center justify-center">
	<form method="POST" {@attach enhanceAttachment} class="w-full md:max-w-xl">
		<CreateFormCard
			title="Create thread"
			description="Fill out the form to create a new thread."
			{cancelHref}
		>
			<Form.Field {form} name="categoryId" class="flex flex-col">
				<RemoteCombobox
					bind:value={$formData.categoryId}
					label="Category"
					placeholder="Select a category…"
					searchPlaceholder="Search categories…"
					emptyText="No categories found"
					initialOptions={data.options}
					loadOptions={loadCategories}
				/>
				<Form.FieldErrors />
			</Form.Field>
			<Form.Field {form} name="title">
				<Form.Control>
					{#snippet children({ props })}
						<Form.Label>Thread title</Form.Label>
						<Input {...props} bind:value={$formData.title} />
					{/snippet}
				</Form.Control>
				<Form.FieldErrors />
			</Form.Field>
		</CreateFormCard>
	</form>
</div>
