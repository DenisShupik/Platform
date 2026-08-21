import { PUBLIC_CSR_API_URL } from '$env/static/public'
import { authClient } from '$lib/client'
import { applyLocaleRequestHeader } from '$lib/client/locale-request'
import { client } from '$lib/utils/client/client.gen'
import '@valibot/i18n/ru'

client.setConfig({
	baseUrl: PUBLIC_CSR_API_URL,
	querySerializer: { array: { explode: false } }
})

client.interceptors.request.use(async (request, options) => {
	applyLocaleRequestHeader(request)

	if (!options.security || options.security.length === 0) {
		return request
	}
	const accessToken = (await authClient.getAccessToken({ useAccountCookie: true })).data
		?.accessToken

	if (!accessToken) return request

	request.headers.set('Authorization', `Bearer ${accessToken}`)
	return request
})
