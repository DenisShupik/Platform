<script lang="ts">
	//import MainNavToolBar from '$lib/components/MainNavToolBar.svelte'
	//import RouteLink from '$lib/components/ui/route-link/RouteLink.svelte'
	import { IconSearch, IconUserCircle } from '@tabler/icons-svelte'
	import { Button } from '$lib/components/ui/button'
	import * as DropdownMenu from '$lib/components/ui/dropdown-menu'
	import { Input } from '$lib/components/ui/input'
	import { authState, getCurrentUser } from '$lib/client/auth-state.svelte'
	//import { MainNav, MobileNav, ModeToggle } from '$lib/components'
	import { ModeToggle } from '$lib/components/app'
	import * as Avatar from '$lib/components/ui/avatar'
</script>

<header
	class="border-border/40 bg-background/95 supports-[backdrop-filter]:bg-background/60 sticky top-0 z-50 w-full border-b backdrop-blur"
>
	<div class="container flex h-14 max-w-screen-2xl items-center">
		<!-- <MainNav />
		<MobileNav /> -->
		<div class="flex flex-1 items-center justify-between gap-x-2 md:justify-end md:gap-x-4">
			<!-- <div class="ml-auto mr-2"><MainNavToolBar /></div> -->
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
								{#if authState.keycloak.authenticated}
									<Avatar.Root class="size-8">
										<Avatar.Image src={getCurrentUser()?.avatarUrl} alt="@shadcn" />
										<Avatar.Fallback
											>{authState.keycloak.tokenParsed?.preferred_username}</Avatar.Fallback
										>
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
							{#if authState.keycloak.authenticated}
								<DropdownMenu.GroupHeading
									><div class="flex flex-col space-y-1">
										<p class="text-sm font-medium leading-none">
											{authState.keycloak.tokenParsed?.preferred_username}
										</p>
										<p class="text-muted-foreground text-xs leading-none">
											{authState.keycloak.tokenParsed?.email}
										</p>
									</div>
								</DropdownMenu.GroupHeading>
								<DropdownMenu.Item>
									<a href="/settings/profile">Settings</a>
								</DropdownMenu.Item>
								<DropdownMenu.Separator />
								<DropdownMenu.Item onclick={() => authState.keycloak.logout()}
									>Выйти</DropdownMenu.Item
								>
							{:else}
								<DropdownMenu.Item onclick={() => authState.keycloak.login()}
									>Войти</DropdownMenu.Item
								>
							{/if}
						</DropdownMenu.Group>
					</DropdownMenu.Content>
				</DropdownMenu.Root>
				<ModeToggle />
			</nav>
		</div>
	</div>
</header>
