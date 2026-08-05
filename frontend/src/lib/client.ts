import { createAuthClient } from 'better-auth/svelte'
import { genericOAuthClient, inferAdditionalFields } from 'better-auth/client/plugins'
import type { auth } from './auth'
import type { UserId } from './utils/client'

const rawAuthClient = createAuthClient({
	plugins: [genericOAuthClient(), inferAdditionalFields<typeof auth>()]
})

type RawSessionStore = ReturnType<typeof rawAuthClient.useSession>
type RawSessionState = ReturnType<RawSessionStore['get']>
type BrandedSessionData<Data> = Data extends { user: infer User }
	? Omit<Data, 'user'> & { user: Omit<User, 'userId'> & { userId: UserId } }
	: Data
type BrandedSessionState = Omit<RawSessionState, 'data'> & {
	data: BrandedSessionData<RawSessionState['data']>
}
type BrandedSessionStore = Omit<RawSessionStore, 'get' | 'subscribe'> & {
	get: () => BrandedSessionState
	subscribe: (run: (value: BrandedSessionState) => void) => () => void
}

type AuthClient = Omit<typeof rawAuthClient, 'useSession'> & {
	useSession: () => BrandedSessionStore
}

export const authClient = rawAuthClient as AuthClient
