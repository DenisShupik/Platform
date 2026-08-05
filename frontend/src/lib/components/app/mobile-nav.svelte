<script lang="ts">
	import { page } from '$app/state'
	import { resolve } from '$app/paths'
	import type { Pathname } from '$app/types'
	import { Button } from '$lib/components/ui/button'
	import * as ScrollArea from '$lib/components/ui/scroll-area'
	import * as Sheet from '$lib/components/ui/sheet'
	import { appNavigation } from '$lib/client/routes'
	import { authClient } from '$lib/client'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import IconMenu2 from '~icons/tabler/menu-2'
	import IconMessageCircleFilled from '~icons/tabler/message-circle-filled'

	const session = authClient.useSession()

	function isActive(href: Pathname) {
		return page.url.pathname === resolve(href)
	}
</script>

<Sheet.Root>
	<Sheet.Trigger>
		{#snippet child({ props })}
			<Button
				{...props}
				variant="ghost"
				size="icon"
				class="mr-2 md:hidden"
				aria-label="Открыть навигацию"
			>
				<IconMenu2 data-icon />
			</Button>
		{/snippet}
	</Sheet.Trigger>
	<Sheet.Content side="left" class="w-[min(20rem,calc(100vw-2rem))] gap-0 p-0">
		<Sheet.Header class="sr-only">
			<Sheet.Title>Навигация</Sheet.Title>
			<Sheet.Description>Основные разделы форума.</Sheet.Description>
		</Sheet.Header>
		<div class="flex min-h-0 flex-1 flex-col">
			<div class="px-6 pt-6">
				<Sheet.Close>
					{#snippet child({ props })}
						<a {...props} href={resolve('/')} class="flex items-center gap-2">
							<IconMessageCircleFilled class="size-4" />
							<span class="font-bold">{PUBLIC_APP_NAME}</span>
						</a>
					{/snippet}
				</Sheet.Close>
			</div>
			<ScrollArea.Root class="min-h-0 flex-1">
				<nav class="flex flex-col gap-3 px-6 py-6" aria-label="Основная навигация">
					{#each appNavigation.primary as navItem (navItem.href)}
						{#if !navItem.requiresAuth || $session.data}
							{@const active = isActive(navItem.href)}
							<Sheet.Close>
								{#snippet child({ props })}
									<a
										{...props}
										href={resolve(navItem.href)}
										class={active ? 'font-medium text-foreground' : 'text-foreground/70'}
										aria-current={active ? 'page' : undefined}
									>
										{navItem.title}
									</a>
								{/snippet}
							</Sheet.Close>
						{/if}
					{/each}
				</nav>
			</ScrollArea.Root>
		</div>
	</Sheet.Content>
</Sheet.Root>
