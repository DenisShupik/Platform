<script lang="ts">
	import { resolve } from '$app/paths'
	import { page } from '$app/state'
	import type { Pathname } from '$app/types'
	import * as Sidebar from '$lib/components/ui/sidebar'
	import { cn } from '$lib/utils.js'

	type SidebarItem = { href: Pathname; title: string }

	let {
		items,
		label = 'Settings',
		class: className
	}: { items: SidebarItem[]; label?: string; class?: string | null } = $props()

	function isActive(href: Pathname) {
		return page.url.pathname === resolve(href)
	}
</script>

<aside class="hidden shrink-0 lg:block lg:w-56">
	<Sidebar.Root collapsible="none" class={cn('h-auto w-full rounded-lg border', className)}>
		<Sidebar.Header>
			<h2 class="px-2 text-sm font-semibold">{label}</h2>
		</Sidebar.Header>
		<Sidebar.Content>
			<Sidebar.Group>
				<Sidebar.GroupContent>
					<Sidebar.Menu>
						{#each items as item (item.href)}
							{@const active = isActive(item.href)}
							<Sidebar.MenuItem>
								<Sidebar.MenuButton isActive={active}>
									{#snippet child({ props })}
										<a
											{...props}
											href={resolve(item.href)}
											aria-current={active ? 'page' : undefined}
										>
											<span>{item.title}</span>
										</a>
									{/snippet}
								</Sidebar.MenuButton>
							</Sidebar.MenuItem>
						{/each}
					</Sidebar.Menu>
				</Sidebar.GroupContent>
			</Sidebar.Group>
		</Sidebar.Content>
	</Sidebar.Root>
</aside>

<nav
	class={cn('no-scrollbar flex gap-1 overflow-x-auto border-b pb-3 lg:hidden', className)}
	aria-label={label}
>
	{#each items as item (item.href)}
		{@const active = isActive(item.href)}
		<a
			href={resolve(item.href)}
			class={cn(
				'rounded-md px-3 py-2 text-sm font-medium whitespace-nowrap transition-colors',
				active
					? 'bg-muted text-foreground'
					: 'text-muted-foreground hover:bg-muted hover:text-foreground'
			)}
			aria-current={active ? 'page' : undefined}
		>
			{item.title}
		</a>
	{/each}
</nav>
