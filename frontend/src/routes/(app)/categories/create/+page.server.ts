import { vCreateCategoryRequestBody } from '$lib/utils/client/valibot.gen'
import { fail, superValidate } from 'sveltekit-superforms'
import { valibot } from 'sveltekit-superforms/adapters'
import type { PageServerLoad } from './$types'
import { createCategory, getForum, type ForumId } from '$lib/utils/client'
import { redirect, type Actions } from '@sveltejs/kit'
import { resolve } from '$app/paths'
import { canCreateCategoryPolicy } from '$lib/roles'
import { transformToOptions, type Option } from './utils'
import { parseCategoryTitle, parseForumId } from '$lib/utils/value-object'

export const load: PageServerLoad = async ({ url, locals }) => {
	const auth = locals.accessToken

	const canCreateCategory = canCreateCategoryPolicy(locals.role)

	let initialData: { forumId?: ForumId }
	let options: Option[]

	const searchParam = url.searchParams.get('forumId')
	const forumId = parseForumId(searchParam)

	if (forumId !== undefined) {
		const forum = (
			await getForum<true>({
				path: { forumId },
				auth
			})
		).data

		options = transformToOptions([forum])

		initialData = { forumId }
	} else {
		options = []
		initialData = {}
	}

	return {
		canCreateCategory,
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

		const auth = locals.accessToken
		const forumId = parseForumId(form.data.forumId)
		const title = parseCategoryTitle(form.data.title)
		if (forumId === undefined || title === undefined) return fail(400, { form })

		const result = await createCategory<true>({
			body: {
				forumId,
				title
			},
			auth
		})

		throw redirect(
			303,
			resolve('/(app)/categories/[categoryId=CategoryId]', { categoryId: result.data })
		)
	}
}
