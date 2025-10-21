import { vCategoryId, vCreateThreadRequestBody } from '$lib/utils/client/valibot.gen'
import { createThread, getCategory, getPoliciesBulk, type CategoryId } from '$lib/utils/client'
import { safeParse } from 'valibot'
import type { Actions, PageServerLoad } from './$types'
import { fail, superValidate } from 'sveltekit-superforms'
import { valibot } from 'sveltekit-superforms/adapters'
import { transformToOptions, type Option } from '../../../api/categories/utils'
import { redirect } from '@sveltejs/kit'
import { resolve } from '$app/paths'

export const load: PageServerLoad = async ({ url, locals }) => {
	const session = await locals.auth()
	const auth = session?.access_token

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

		const policies = (
			await getPoliciesBulk<true>({
				path: {
					policyIds: [category.readPolicyId, category.postCreatePolicyId]
				}
			})
		).data

		options = transformToOptions([category], policies)
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

		const session = await locals.auth()
		const auth = session?.access_token

		const result = await createThread<true>({
			body: {
				categoryId: form.data.categoryId,
				title: form.data.title,
				readPolicyValue: form.data.readPolicyValue,
				postCreatePolicyValue: form.data.postCreatePolicyValue
			},
			auth
		})

		throw redirect(303, resolve('/(app)/threads/[threadId=ThreadId]', { threadId: result.data }))
	}
}
