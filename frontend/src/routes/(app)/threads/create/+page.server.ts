import { withApiLocale } from '$lib/client/api-options'
import { vCreateThreadRequestBody } from '$lib/utils/client/valibot.gen'
import { createThread, getCategory, type CategoryId } from '$lib/utils/client'
import type { Actions, PageServerLoad } from './$types'
import { fail, superValidate } from 'sveltekit-superforms'
import { valibot } from 'sveltekit-superforms/adapters'
import { transformToOptions, type Option } from './utils'
import { redirect } from '@sveltejs/kit'
import { parseCategoryId, parseThreadTitle } from '$lib/utils/value-object'
import { getLocale } from '$lib/paraglide/runtime'
import { resolve } from '$app/paths'

export const load: PageServerLoad = async ({ url, locals }) => {
	const auth = locals.accessToken

	let initialData: { categoryId?: CategoryId }
	let options: Option[]

	const searchParam = url.searchParams.get('categoryId')
	const categoryId = parseCategoryId(searchParam)

	if (categoryId !== undefined) {
		const category = (
			await getCategory<true>(
				withApiLocale({
					path: { categoryId },
					auth,
					throwOnError: true
				})
			)
		).data
		options = transformToOptions([category])
		initialData = { categoryId }
	} else {
		options = []
		initialData = {}
	}

	return {
		options,
		form: await superValidate(
			initialData,
			valibot(vCreateThreadRequestBody, { config: { lang: getLocale() } }),
			{ errors: false }
		)
	}
}

export const actions: Actions = {
	default: async ({ request, locals }) => {
		const form = await superValidate(
			request,
			valibot(vCreateThreadRequestBody, { config: { lang: getLocale() } })
		)

		if (!form.valid) {
			return fail(400, { form })
		}

		const auth = locals.accessToken
		const categoryId = parseCategoryId(form.data.categoryId)
		const title = parseThreadTitle(form.data.title)
		if (categoryId === undefined || title === undefined) return fail(400, { form })

		const result = await createThread<true>(
			withApiLocale({
				body: {
					categoryId,
					title
				},
				auth,
				throwOnError: true
			})
		)

		throw redirect(303, resolve('/(app)/threads/[threadId=ThreadId]', { threadId: result.data }))
	}
}
