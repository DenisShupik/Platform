<script lang="ts">
	import type { Pathname } from '$app/types'
	import { SidebarNav } from '$lib/components/app'

	let { children } = $props()

	const sidebarNavItems: { title: string; href: Pathname }[] = [
		{
			title: 'Profile',
			href: '/settings/profile'
		}
		// ,{
		// 	title: 'Notifications',
		// 	href: '/settings/notifications'
		// }
	]
</script>

<div class="flex flex-col gap-8 lg:flex-row lg:gap-12 lg:px-24">
	<aside class="mx-4 lg:w-1/5">
		<SidebarNav items={sidebarNavItems} />
	</aside>
	<div class="w-full">
		{@render children()}
	</div>
</div>
