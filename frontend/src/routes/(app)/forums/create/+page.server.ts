import { withApiLocale } from '$lib/client/api-options'
import { fail, superValidate } from 'sveltekit-superforms'
import type { Actions, PageServerLoad } from './$types'
import { vCreateForumRequestBody } from '$lib/utils/client/valibot.gen'
import { valibot } from 'sveltekit-superforms/adapters'
import { error, redirect } from '@sveltejs/kit'
import { createForum } from '$lib/utils/client'
import { parseForumTitle } from '$lib/utils/value-object'
import { getLocale } from '$lib/paraglide/runtime'
import { resolve } from '$app/paths'

export const load: PageServerLoad = async ({ locals, parent }) => {
	const { platformAllowedActions } = await parent()
	if (!locals.accessToken || !platformAllowedActions.canManageStructure) error(403)

	return {
		form: await superValidate(valibot(vCreateForumRequestBody, { config: { lang: getLocale() } }))
	}
}

export const actions: Actions = {
	default: async ({ request, locals }) => {
		const form = await superValidate(
			request,
			valibot(vCreateForumRequestBody, { config: { lang: getLocale() } })
		)

		if (!form.valid) {
			return fail(400, { form })
		}

		const auth = locals.accessToken
		const title = parseForumTitle(form.data.title)
		if (title === undefined) return fail(400, { form })

		const result = await createForum<true>(
			withApiLocale({
				body: {
					title
				},
				auth,
				throwOnError: true
			})
		)

		redirect(303, resolve('/(app)/forums/[forumId=ForumId]', { forumId: result.data }))
	}
}
