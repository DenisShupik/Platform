import { vCategoryId, vCreateThreadRequestBody } from '$lib/utils/client/valibot.gen'
import { createThread, getCategory, type CategoryId } from '$lib/utils/client'
import { safeParse } from 'valibot'
import type { Actions, PageServerLoad } from './$types'
import { fail, superValidate } from 'sveltekit-superforms'
import { valibot } from 'sveltekit-superforms/adapters'
import { transformToOptions, type Option } from './utils'
import { redirect } from '@sveltejs/kit'
import { resolve } from '$app/paths'

export const load: PageServerLoad = async ({ url, locals }) => {
	const auth = locals.accessToken

	let initialData: { categoryId?: CategoryId }
	let options: Option[]

	const searchParam = url.searchParams.get('categoryId')
	const parseResult = safeParse(vCategoryId, searchParam)

	if (parseResult.success) {
		const categoryId = parseResult.output
		const category = (
			await getCategory<true>({
				path: { categoryId },
				auth
			})
		).data
		options = transformToOptions([category])
		initialData = { categoryId }
	} else {
		options = []
		initialData = {}
	}

	return {
		options,
		form: await superValidate(initialData, valibot(vCreateThreadRequestBody), { errors: false })
	}
}

export const actions: Actions = {
	default: async ({ request, locals }) => {
		const form = await superValidate(request, valibot(vCreateThreadRequestBody))

		if (!form.valid) {
			return fail(400, { form })
		}

		const auth = locals.accessToken

		const result = await createThread<true>({
			body: {
				categoryId: form.data.categoryId,
				title: form.data.title
			},
			auth
		})

		throw redirect(303, resolve('/(app)/threads/[threadId=ThreadId]', { threadId: result.data }))
	}
}
