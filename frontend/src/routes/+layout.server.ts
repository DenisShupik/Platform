import { env } from '$env/dynamic/public'
import { client } from '$lib/utils/client/client.gen'

client.setConfig({
	baseUrl: env.PUBLIC_SSR_API_URL,
	querySerializer: { array: { explode: false } }
})
