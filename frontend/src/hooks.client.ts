import { PUBLIC_API_URL } from '$env/static/public'
import { client } from '$lib/utils/client/client.gen'

client.setConfig({
	baseUrl: PUBLIC_API_URL,
	querySerializer: { array: { explode: false } }
})
