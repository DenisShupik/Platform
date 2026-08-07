import type { Pathname } from '$app/types'

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
			title: 'Watched',
			href: '/current-user/watched',
			requiresAuth: true
		},
		{
			title: 'Bookmarks',
			href: '/current-user/bookmarks',
			requiresAuth: true
		}
	],
	settings: [
		{
			title: 'Profile',
			href: '/settings/profile'
		}
	]
}
