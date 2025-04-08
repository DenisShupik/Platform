export interface NavItem {
	title: string
	href?: string
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
