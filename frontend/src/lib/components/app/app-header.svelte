<script lang="ts">
	import IconCategoryPlus from '~icons/tabler/category-plus'
	import IconEdit from '~icons/tabler/edit'
	import IconFolderPlus from '~icons/tabler/folder-plus'
	import IconLogin2 from '~icons/tabler/login-2'
	import IconLogout2 from '~icons/tabler/logout-2'
	import IconSearch from '~icons/tabler/search'
	import IconSettings from '~icons/tabler/settings'
	import IconTextPlus from '~icons/tabler/text-plus'
	import IconUserCircle from '~icons/tabler/user-circle'
	import { Button } from '$lib/components/ui/button'
	import * as DropdownMenu from '$lib/components/ui/dropdown-menu'
	import { Input } from '$lib/components/ui/input'
	import { MainNav, MobileNav, ModeToggle, NotificationMenu } from '$lib/components/app'
	import * as Avatar from '$lib/components/ui/avatar'
	import { resolve } from '$app/paths'
	import { authClient } from '$lib/client'
	import { canCreateCategoryPolicy, canCreateForumPolicy, canCreateThreadPolicy } from '$lib/roles'

	const session = authClient.useSession()

	const permissions = $derived.by(() => {
		const role = $session.data?.user.role
		return {
			canCreateForum: canCreateForumPolicy(role),
			canCreateCategory: canCreateCategoryPolicy(role),
			canCreateThread: canCreateThreadPolicy(role)
		}
	})

	let appBarHeight = $state(0)

	$effect(() => {
		document.documentElement.style.setProperty('--app-bar-height', appBarHeight + 8 + 'px')
	})
</script>

<header
	bind:clientHeight={appBarHeight}
	class="sticky top-0 z-50 w-full border-b border-border/40 bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60"
>
	<div class="mx-auto flex h-14 w-full max-w-(--breakpoint-2xl) items-center px-4">
		<MainNav />
		<MobileNav />
		<div class="flex flex-1 items-center justify-between gap-x-2 md:justify-end md:gap-x-4">
			<form class="flex-1 sm:flex-initial">
				<div class="relative">
					<IconSearch class="absolute top-2.5 left-2.5 h-4 w-4 text-muted-foreground" />
					<Input type="search" placeholder="Поиск..." class="pl-8 sm:w-75 md:w-50 lg:w-75" />
				</div>
			</form>
			<nav class="flex items-center gap-x-2">
				<DropdownMenu.Root>
					<DropdownMenu.Trigger>
						{#snippet child({ props })}
							<Button {...props} variant="outline" size="icon" class="relative size-8 rounded-full">
								{#if $session.data}
									<Avatar.Root class="size-8">
										<Avatar.Image src={$session.data.user.avatarUrl} alt="@shadcn" />
										<Avatar.Fallback>{$session.data.user.name}</Avatar.Fallback>
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
							{#if $session.data}
								<DropdownMenu.GroupHeading
									><div class="flex flex-col space-y-1">
										<p class="text-sm leading-none font-medium">
											{$session.data.user.name}
										</p>
										<p class="text-xs leading-none text-muted-foreground">
											{$session.data.user.email}
										</p>
									</div>
								</DropdownMenu.GroupHeading>
								<DropdownMenu.Separator />
								{#if permissions.canCreateForum}
									<DropdownMenu.Item>
										<IconFolderPlus class="mr-1 size-4" />
										<a href={resolve('/(app)/forums/create')}>Create forum</a>
									</DropdownMenu.Item>
								{/if}
								{#if permissions.canCreateCategory}
									<DropdownMenu.Item>
										<IconCategoryPlus class="mr-1 size-4" />
										<a href={resolve('/(app)/categories/create')}>Create category</a>
									</DropdownMenu.Item>
								{/if}
								{#if permissions.canCreateThread}
									<DropdownMenu.Item>
										<IconTextPlus class="mr-1 size-4" />
										<a href={resolve('/(app)/threads/create')}>Create thread</a>
									</DropdownMenu.Item>
								{/if}
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
								<DropdownMenu.Item
									onclick={async () => {
										await authClient.signOut()
									}}><IconLogout2 class="mr-1 size-4" />Logout</DropdownMenu.Item
								>
							{:else}
								<DropdownMenu.Item
									onclick={async () => {
										await authClient.signIn.oauth2({
											providerId: 'keycloak'
										})
									}}
								>
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
