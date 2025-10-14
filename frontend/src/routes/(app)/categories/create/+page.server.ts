import { vCreateCategoryRequestBody, vForumId } from '$lib/utils/client/valibot.gen'
import { fail, superValidate } from 'sveltekit-superforms'
import { valibot } from 'sveltekit-superforms/adapters'
import type { PageServerLoad } from './$types'
import { safeParse } from 'valibot'
import { createCategory, getForum, type ForumId, type ForumTitle } from '$lib/utils/client'
import { redirect, type Actions } from '@sveltejs/kit'
import { resolve } from '$app/paths'

export const load: PageServerLoad = async ({ url, locals }) => {
	const session = await locals.auth()
	const auth = session?.access_token

	let initialData: { forumId?: ForumId }
	let options: { label: ForumTitle; value: ForumId }[]

	const searchParam = url.searchParams.get('forumId')
	const parseResult = safeParse(vForumId, searchParam)

	if (parseResult.success) {
		const forumId = parseResult.output
		const forum = (
			await getForum({
				path: { forumId },
				auth
			})
		).data

		options = [
			{
				label: forum.title,
				value: forumId
			}
		]
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

		const result = await createCategory({
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
