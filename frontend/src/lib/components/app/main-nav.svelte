<script lang="ts">
	import { page } from '$app/state'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import { resolve } from '$app/paths'
	import type { Pathname } from '$app/types'
	import { authClient } from '$lib/client'
	import { appNavigation } from '$lib/client/routes'
	import { cn } from '$lib/utils.js'
	import IconMessageCircleFilled from '~icons/tabler/message-circle-filled'

	const forumsHref = resolve('/')
	const session = authClient.useSession()

	function isActive(href: Pathname) {
		return page.url.pathname === resolve(href)
	}
</script>

<div class="hidden items-center gap-6 md:flex">
	<a href={forumsHref} class="flex items-center gap-2" aria-label={PUBLIC_APP_NAME}>
		<IconMessageCircleFilled class="size-6" />
		<span class="hidden font-bold xl:inline-block">
			{PUBLIC_APP_NAME}
		</span>
	</a>
	<nav class="flex items-center gap-6 text-sm" aria-label="Основная навигация">
		{#each appNavigation.primary as navItem (navItem.href)}
			{#if !navItem.requiresAuth || $session.data}
				<a
					href={resolve(navItem.href)}
					class={cn(
						'transition-colors hover:text-foreground/80',
						isActive(navItem.href) ? 'text-foreground' : 'text-foreground/60'
					)}
					aria-current={isActive(navItem.href) ? 'page' : undefined}
					>{navItem.title}
				</a>
			{/if}
		{/each}
	</nav>
</div>
