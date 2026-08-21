import { createAuth, type Auth } from '$lib/auth'

let initializedAuth: Promise<Auth> | undefined

export function getAuth(): Promise<Auth> {
	initializedAuth ??= initializeAuth()
	return initializedAuth
}

async function initializeAuth(): Promise<Auth> {
	const auth = createAuth()

	try {
		await auth.$context
		return auth
	} catch (error) {
		initializedAuth = undefined
		throw error
	}
}
