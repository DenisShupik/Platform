import { authErrorBridgePath } from '$lib/auth-locale'

export { authErrorBridgePath }

export function buildAuthErrorUrl(requestUrl: URL): URL {
	const target = new URL('/auth/error', requestUrl.origin)
	const error = requestUrl.searchParams.get('error')

	if (error) target.searchParams.set('error', error)

	return target
}
