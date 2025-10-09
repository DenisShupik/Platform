<script lang="ts">
	import {
		IconCategoryPlus,
		IconEdit,
		IconFolderPlus,
		IconLogin2,
		IconLogout2,
		IconSearch,
		IconSettings,
		IconTextPlus,
		IconUserCircle
	} from '@tabler/icons-svelte'
	import { Button } from '$lib/components/ui/button'
	import * as DropdownMenu from '$lib/components/ui/dropdown-menu'
	import { Input } from '$lib/components/ui/input'
	import { MainNav, MobileNav, ModeToggle, NotificationMenu } from '$lib/components/app'
	import * as Avatar from '$lib/components/ui/avatar'
	import { resolve } from '$app/paths'
	import { signIn, signOut } from '@auth/sveltekit/client'
	import { page } from '$app/state'

	let appBarHeight = $state(0)

	$effect(() => {
		document.documentElement.style.setProperty('--app-bar-height', appBarHeight + 8 + 'px')
	})
</script>

<header
	bind:clientHeight={appBarHeight}
	class="border-border/40 bg-background/95 supports-backdrop-filter:bg-background/60 sticky top-0 z-50 w-full border-b backdrop-blur"
>
	<div class="max-w-(--breakpoint-2xl) container flex h-14 items-center">
		<MainNav />
		<MobileNav />
		<div class="flex flex-1 items-center justify-between gap-x-2 md:justify-end md:gap-x-4">
			<form class="flex-1 sm:flex-initial">
				<div class="relative">
					<IconSearch class="text-muted-foreground absolute left-2.5 top-2.5 h-4 w-4" />
					<Input
						type="search"
						placeholder="Поиск..."
						class="pl-8 sm:w-[300px] md:w-[200px] lg:w-[300px]"
					/>
				</div>
			</form>
			<nav class="flex items-center gap-x-2">
				<DropdownMenu.Root>
					<DropdownMenu.Trigger>
						{#snippet child({ props })}
							<Button {...props} variant="outline" size="icon" class="relative size-8 rounded-full">
								{#if page.data.session}
									<Avatar.Root class="size-8">
										<Avatar.Image src={page.data.session.user?.avatarUrl} alt="@shadcn" />
										<Avatar.Fallback>{page.data.session.user?.name}</Avatar.Fallback>
									</Avatar.Root>
								{:else}
									<IconUserCircle />
								{/if}
								<span class="sr-only">Toggle user menu</span>
							</Button>
						{/snippet}
					</DropdownMenu.Trigger>
					<DropdownMenu.Content align="end">
						<DropdownMenu.Group>
							{#if page.data.session}
								<DropdownMenu.GroupHeading
									><div class="flex flex-col space-y-1">
										<p class="text-sm font-medium leading-none">
											{page.data.session.user?.name}
										</p>
										<p class="text-muted-foreground text-xs leading-none">
											{page.data.session.user?.email}
										</p>
									</div>
								</DropdownMenu.GroupHeading>
								<DropdownMenu.Separator />
								<DropdownMenu.Item>
									<IconFolderPlus class="mr-1 size-4" />
									<a href={resolve('/(app)/forums/create')}>Create forum</a>
								</DropdownMenu.Item>
								<DropdownMenu.Item>
									<IconCategoryPlus class="mr-1 size-4" />
									<a href={resolve('/(app)/categories/create')}>Create category</a>
								</DropdownMenu.Item>
								<DropdownMenu.Item>
									<IconTextPlus class="mr-1 size-4" />
									<a href={resolve('/(app)/threads/create')}>Create thread</a>
								</DropdownMenu.Item>
								<DropdownMenu.Separator />
								<DropdownMenu.Item>
									<IconEdit class="mr-1 size-4" />
									<a href={resolve('/(app)/current-user/thread-drafts')}>Thread drafts</a>
								</DropdownMenu.Item>
								<DropdownMenu.Separator />
								<DropdownMenu.Item>
									<IconSettings class="mr-1 size-4" />
									<a href={resolve('/(app)/settings/profile')}>Settings</a>
								</DropdownMenu.Item>
								<DropdownMenu.Separator />
								<DropdownMenu.Item onclick={() => signOut()}
									><IconLogout2 class="mr-1 size-4" />Logout</DropdownMenu.Item
								>
							{:else}
								<DropdownMenu.Item onclick={() => signIn('keycloak')}>
									<IconLogin2 class="mr-1 size-4" />Login</DropdownMenu.Item
								>
							{/if}
						</DropdownMenu.Group>
					</DropdownMenu.Content>
				</DropdownMenu.Root>
				<NotificationMenu />
				<ModeToggle />
			</nav>
		</div>
	</div>
</header>

<style>
	:global(html) {
		scroll-padding-top: var(--app-bar-height, 0px);
	}
</style>
