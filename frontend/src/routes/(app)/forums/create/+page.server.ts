import { fail, superValidate } from 'sveltekit-superforms'
import type { Actions, PageServerLoad } from './$types'
import { vCreateForumRequestBody } from '$lib/utils/client/valibot.gen'
import { valibot } from 'sveltekit-superforms/adapters'
import { redirect } from '@sveltejs/kit'
import { resolve } from '$app/paths'
import { createForum } from '$lib/utils/client'
import { parseForumTitle } from '$lib/utils/value-object'

export const load: PageServerLoad = async () => {
	// TODO: сделать проверку что пользователь может создавать форумы

	return {
		form: await superValidate(valibot(vCreateForumRequestBody))
	}
}

export const actions: Actions = {
	default: async ({ request, locals }) => {
		const form = await superValidate(request, valibot(vCreateForumRequestBody))

		if (!form.valid) {
			return fail(400, { form })
		}

		const auth = locals.accessToken
		const title = parseForumTitle(form.data.title)
		if (title === undefined) return fail(400, { form })

		const result = await createForum<true>({
			body: {
				title
			},
			auth
		})

		throw redirect(303, resolve('/(app)/forums/[forumId=ForumId]', { forumId: result.data }))
	}
}
