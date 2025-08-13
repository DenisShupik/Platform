<script lang="ts">
	import { currentUser, login } from '$lib/client/current-user-state.svelte'
	import { SidebarNav } from '$lib/components/app'

	let { children } = $props()

	const sidebarNavItems = [
		{
			title: 'Profile',
			href: '/settings/profile'
		}
		// ,{
		// 	title: 'Notifications',
		// 	href: '/settings/notifications'
		// }
	]

	$effect(() => {
		if (!currentUser.user) {
			login()
		}
	})
</script>

<div class="flex flex-col space-y-8 lg:flex-row lg:space-x-12 lg:space-y-0 lg:px-24">
	<aside class="mx-4 lg:w-1/5">
		<SidebarNav items={sidebarNavItems} />
	</aside>
	<div class="w-full">
		{@render children()}
	</div>
</div>
