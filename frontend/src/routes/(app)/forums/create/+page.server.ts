import { fail, superValidate } from 'sveltekit-superforms'
import type { Actions, PageServerLoad } from './$types'
import { vCreateForumRequestBody } from '$lib/utils/client/valibot.gen'
import { valibot } from 'sveltekit-superforms/adapters'
import { redirect } from '@sveltejs/kit'
import { resolve } from '$app/paths'
import { createForum, getPortal } from '$lib/utils/client'

export const load: PageServerLoad = async () => {
	const portal = (await getPortal()).data

	return {
		portal,
		form: await superValidate(valibot(vCreateForumRequestBody))
	}
}

export const actions: Actions = {
	default: async ({ request, locals }) => {
		const form = await superValidate(request, valibot(vCreateForumRequestBody))

		if (!form.valid) {
			return fail(400, { form })
		}

		const session = await locals.auth()
		const auth = session?.access_token

		const result = await createForum({
			body: {
				title: form.data.title,
				readPolicyValue: form.data.readPolicyValue,
				categoryCreatePolicyValue: form.data.categoryCreatePolicyValue,
				threadCreatePolicyValue: form.data.threadCreatePolicyValue,
				postCreatePolicyValue: form.data.postCreatePolicyValue
			},
			auth
		})

		throw redirect(303, resolve('/(app)/forums/[forumId=ForumId]', { forumId: result.data }))
	}
}
