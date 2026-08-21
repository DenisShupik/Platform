// See https://svelte.dev/docs/kit/types#app.d.ts
// for information about these interfaces

import 'unplugin-icons/types/svelte'
import type { UserId } from '$lib/utils/client'

declare global {
	namespace App {
		// interface Error {}
		interface Locals {
			accessToken?: string
			userId?: UserId
		}
		// interface PageData {}
		// interface PageState {}
		// interface Platform {}
	}
}

export {}
