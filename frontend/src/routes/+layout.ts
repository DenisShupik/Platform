import { apiUrl } from '$lib/config/env'
import { getAccessToken } from '$lib/states/authStore'
import { client } from '$lib/utils/client/client.gen'

export const ssr = false

client.setConfig({
  baseUrl: apiUrl,
  querySerializer: { array: { explode: false } }
})

client.interceptors.request.use(async (request, options) => {
  const token = await getAccessToken()
  request.headers.set('Authorization', token)
  return request
})
