import type { RequestHandler } from './$types'
import { getCategoriesPaged, getPoliciesBulk } from '$lib/utils/client'
import { error } from '@sveltejs/kit'
import { transformToOptions } from './utils'

export const GET: RequestHandler = async ({ url, locals }) => {
	const title = url.searchParams.get('title')

	const session = await locals.auth()
	const auth = session?.access_token
	if (auth) {
		const result = (
			await getCategoriesPaged<true>({
				query: { title },
				auth
			})
		).data

		const policyIds = result.flatMap((e) => [
			e.readPolicyId,
			e.postCreatePolicyId
		])

		const policies = (
			await getPoliciesBulk<true>({
				path: {
					policyIds
				}
			})
		).data

		const options = transformToOptions(result, policies)

		return new Response(JSON.stringify(options), {
			headers: { 'Content-Type': 'application/json' }
		})
	} else {
		error(401, 'Unauthorized')
	}
}
