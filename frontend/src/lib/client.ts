import { createAuthClient } from 'better-auth/svelte'
import { genericOAuthClient, inferAdditionalFields } from 'better-auth/client/plugins'
import type { auth } from './auth'

export const authClient = createAuthClient({
	plugins: [genericOAuthClient(), inferAdditionalFields<typeof auth>()]
})
