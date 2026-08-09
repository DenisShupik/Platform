import type { Pathname } from '$app/types'
import * as m from '$lib/paraglide/messages'

export interface NavigationItem {
	title: string
	href: Pathname
	requiresAuth?: boolean
}

interface AppNavigation {
	primary: NavigationItem[]
	settings: NavigationItem[]
}

export const appNavigation: AppNavigation = {
	primary: [
		{
			get title() {
				return m.nav_watched()
			},
			href: '/current-user/watched',
			requiresAuth: true
		},
		{
			get title() {
				return m.nav_bookmarks()
			},
			href: '/current-user/bookmarks',
			requiresAuth: true
		}
	],
	settings: [
		{
			get title() {
				return m.nav_profile()
			},
			href: '/settings/profile'
		}
	]
}
