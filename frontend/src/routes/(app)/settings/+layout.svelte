<script lang="ts">
	import { appNavigation } from '$lib/client/routes'
	import { SidebarNav } from '$lib/components/app'
	import * as Sidebar from '$lib/components/ui/sidebar'

	let { children } = $props()
</script>

<Sidebar.Provider class="min-h-0 flex-col gap-8 lg:flex-row lg:gap-12 lg:px-24">
	<SidebarNav items={appNavigation.settings} />
	<div class="min-w-0 flex-1">
		{@render children()}
	</div>
</Sidebar.Provider>
