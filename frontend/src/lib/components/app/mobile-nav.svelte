<script lang="ts">
	import * as Sheet from '$lib/components/ui/sheet/index.js'
	import { Badge } from '$lib/components/ui/badge/index.js'
	import { Button } from '$lib/components/ui/button/index.js'
	import { ScrollArea } from '$lib/components/ui/scroll-area/index.js'
	import { docsConfig } from '$lib/client/routes'
	import IconMenu2 from '~icons/tabler/menu-2'
	import IconMessageCircleFilled from '~icons/tabler/message-circle-filled'
	import { MobileLink } from '$lib/components/app'
	import { PUBLIC_APP_NAME } from '$env/static/public'

	let open = $state(false)
</script>

<Sheet.Root bind:open>
	<Sheet.Trigger>
		{#snippet child({ props })}
			<Button
				{...props}
				variant="ghost"
				size="icon"
				class="mr-2 md:hidden"
				aria-label="Открыть меню"
			>
				<IconMenu2 data-icon />
			</Button>
		{/snippet}
	</Sheet.Trigger>
	<Sheet.Content side="left" class="p-0">
		<Sheet.Header class="sr-only">
			<Sheet.Title>Навигация</Sheet.Title>
		</Sheet.Header>
		<div class="px-6 pt-6">
			<MobileLink href="/" class="flex items-center gap-2" onNavigate={() => (open = false)}>
				<IconMessageCircleFilled class="size-4" />
				<span class="font-bold">{PUBLIC_APP_NAME}</span>
			</MobileLink>
		</div>
		<ScrollArea class="my-4 h-[calc(100vh-8rem)]">
			<div class="flex flex-col gap-6 px-6 pb-10">
				<nav class="flex flex-col gap-3" aria-label="Основная навигация">
					{#each docsConfig.mainNav as navItem (navItem.href ?? navItem.title)}
						{#if navItem.href}
							<MobileLink
								href={navItem.href}
								onNavigate={() => (open = false)}
								class="text-foreground"
							>
								{navItem.title}
							</MobileLink>
						{/if}
					{/each}
				</nav>
				<nav class="flex flex-col gap-6" aria-label="Разделы">
					{#each docsConfig.sidebarNav as navItem (navItem.href ?? navItem.title)}
						<div class="flex flex-col gap-3">
							<h4 class="font-medium">{navItem.title}</h4>
							{#if navItem?.items?.length}
								{#each navItem.items as item (item.href ?? item.title)}
									{#if !item.disabled && item.href}
										<MobileLink
											href={item.href}
											onNavigate={() => (open = false)}
											class="flex items-center gap-2"
										>
											{item.title}
											{#if item.label}
												<Badge variant="secondary">{item.label}</Badge>
											{/if}
										</MobileLink>
									{/if}
								{/each}
							{/if}
						</div>
					{/each}
				</nav>
			</div>
		</ScrollArea>
	</Sheet.Content>
</Sheet.Root>
