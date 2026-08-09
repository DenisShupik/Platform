import { env } from '$env/dynamic/public'
import { applyLocaleRequestHeader } from '$lib/client/locale-request'
import { client } from '$lib/utils/client/client.gen'

client.setConfig({
	baseUrl: env.PUBLIC_SSR_API_URL,
	querySerializer: { array: { explode: false } },
	throwOnError: true
})

client.interceptors.request.use((request) => {
	return applyLocaleRequestHeader(request)
})
