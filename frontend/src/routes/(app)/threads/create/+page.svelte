<script lang="ts">
	import { withApiLocale } from '$lib/client/api-options'
	import { resolve } from '$app/paths'
	import * as m from '$lib/paraglide/messages'
	import { getLocale } from '$lib/paraglide/runtime'
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
		{ validators: valibot(vCreateThreadRequestBody, { config: { lang: getLocale() } }) }
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

		const categories = (
			await getCategoriesPaged<true>(
				withApiLocale({ query: { title }, signal, throwOnError: true })
			)
		).data
		return transformToOptions(categories)
	}
</script>

<svelte:head>
	<title>{m.thread_create()} — {PUBLIC_APP_NAME}</title>
</svelte:head>

<div class="flex flex-1 items-center justify-center">
	<form method="POST" {@attach enhanceAttachment} class="w-full md:max-w-xl">
		<CreateFormCard
			title={m.thread_create()}
			description={m.thread_create_description()}
			{cancelHref}
		>
			<Form.Field {form} name="categoryId" class="flex flex-col">
				<RemoteCombobox
					bind:value={$formData.categoryId}
					label={m.category()}
					placeholder={m.category_select()}
					searchPlaceholder={m.category_search()}
					emptyText={m.category_none()}
					initialOptions={data.options}
					loadOptions={loadCategories}
				/>
				<Form.FieldErrors />
			</Form.Field>
			<Form.Field {form} name="title">
				<Form.Control>
					{#snippet children({ props })}
						<Form.Label>{m.thread_title()}</Form.Label>
						<Input {...props} bind:value={$formData.title} />
					{/snippet}
				</Form.Control>
				<Form.FieldErrors />
			</Form.Field>
		</CreateFormCard>
	</form>
</div>
