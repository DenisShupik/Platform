import { vCreateCategoryRequestBody, vForumId } from '$lib/utils/client/valibot.gen'
import { fail, superValidate } from 'sveltekit-superforms'
import { valibot } from 'sveltekit-superforms/adapters'
import type { PageServerLoad } from './$types'
import { safeParse } from 'valibot'
import { createCategory, getForum, getPoliciesBulk, type ForumId } from '$lib/utils/client'
import { redirect, type Actions } from '@sveltejs/kit'
import { resolve } from '$app/paths'
import { transformToOptions, type Option } from '../../../api/forums/utils'

export const load: PageServerLoad = async ({ url, locals }) => {
	const session = await locals.auth()
	const auth = session?.access_token

	let initialData: { forumId?: ForumId }
	let options: Option[]

	const searchParam = url.searchParams.get('forumId')
	const parseResult = safeParse(vForumId, searchParam)

	if (parseResult.success) {
		const forumId = parseResult.output
		const forum = (
			await getForum<true>({
				path: { forumId },
				auth
			})
		).data

		const policies = (
			await getPoliciesBulk<true>({
				path: {
					policyIds: [forum.readPolicyId, forum.threadCreatePolicyId, forum.postCreatePolicyId]
				}
			})
		).data

		options = transformToOptions([forum], policies)

		initialData = { forumId }
	} else {
		options = []
		initialData = {}
	}

	return {
		options,
		form: await superValidate(initialData, valibot(vCreateCategoryRequestBody), { errors: false })
	}
}

export const actions: Actions = {
	default: async ({ request, locals }) => {
		const form = await superValidate(request, valibot(vCreateCategoryRequestBody))

		if (!form.valid) {
			return fail(400, { form })
		}

		const session = await locals.auth()
		const auth = session?.access_token

		const result = await createCategory<true>({
			body: {
				forumId: form.data.forumId,
				title: form.data.title,
				readPolicyValue: form.data.readPolicyValue,
				threadCreatePolicyValue: form.data.threadCreatePolicyValue,
				postCreatePolicyValue: form.data.postCreatePolicyValue
			},
			auth
		})

		throw redirect(
			303,
			resolve('/(app)/categories/[categoryId=CategoryId]', { categoryId: result.data })
		)
	}
}
