import { env } from '$env/dynamic/public'
import { applyLocaleRequestHeader } from '$lib/client/locale-request'
import { withApiLocale } from '$lib/client/api-options'
import {
	noAdministrationAllowedActions,
	noPlatformAllowedActions
} from '$lib/category-authorization'
import { getAdministrationAllowedActions, getPlatformAllowedActions } from '$lib/utils/client'
import { client } from '$lib/utils/client/client.gen'
import type { LayoutServerLoad } from './$types'

client.setConfig({
	baseUrl: env.PUBLIC_SSR_API_URL,
	querySerializer: { array: { explode: false } },
	throwOnError: true
})

client.interceptors.request.use((request) => {
	return applyLocaleRequestHeader(request)
})

export const load: LayoutServerLoad = async ({ locals }) => {
	const auth = locals.accessToken
	const [platformAllowedActions, administrationAllowedActions] = auth
		? await Promise.all([
				getPlatformAllowedActions<true>(withApiLocale({ auth, throwOnError: true })).then(
					(response) => response.data
				),
				getAdministrationAllowedActions<true>(withApiLocale({ auth, throwOnError: true })).then(
					(response) => response.data
				)
			])
		: [noPlatformAllowedActions, noAdministrationAllowedActions]

	return { platformAllowedActions, administrationAllowedActions }
}
