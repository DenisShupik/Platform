import type { Pathname } from '$app/types'

export interface NavItem {
	title: string
	href?: Pathname
	disabled?: boolean
	external?: boolean
	label?: string
}

export type SidebarNavItem = NavItem & {
	items: SidebarNavItem[]
}

export type NavItemWithChildren = NavItem & {
	items: NavItemWithChildren[]
}

interface DocsConfig {
	mainNav: NavItem[]
	sidebarNav: SidebarNavItem[]
}

export const docsConfig: DocsConfig = {
	mainNav: [
		{
			title: 'Forums',
			href: '/'
		}
	],
	sidebarNav: []
}
