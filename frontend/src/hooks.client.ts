import { PUBLIC_CSR_API_URL } from '$env/static/public'
import { authClient } from '$lib/client'
import { client } from '$lib/utils/client/client.gen'

client.setConfig({
	baseUrl: PUBLIC_CSR_API_URL,
	querySerializer: { array: { explode: false } }
})

client.interceptors.request.use(async (request, options) => {
	if (!options.security || options.security.length === 0) {
		return request
	}
	console.log('Request requires security, adding access token', request.url)
	const accessToken = (await authClient.getAccessToken({ providerId: 'keycloak' })).data
		?.accessToken

	if (!accessToken) return request

	request.headers.set('Authorization', `Bearer ${accessToken}`)
	return request
})
