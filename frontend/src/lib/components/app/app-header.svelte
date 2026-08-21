<script lang="ts">
	import CirclePlusIcon from '@lucide/svelte/icons/circle-plus'
	import FilePlus2Icon from '@lucide/svelte/icons/file-plus-2'
	import FolderPlusIcon from '@lucide/svelte/icons/folder-plus'
	import LogInIcon from '@lucide/svelte/icons/log-in'
	import LogOutIcon from '@lucide/svelte/icons/log-out'
	import PencilIcon from '@lucide/svelte/icons/pencil'
	import SettingsIcon from '@lucide/svelte/icons/settings'
	import UserCircleIcon from '@lucide/svelte/icons/user-circle'
	import { Button } from '$lib/components/ui/button'
	import * as DropdownMenu from '$lib/components/ui/dropdown-menu'
	import {
		ForumSearch,
		MainNav,
		MobileNav,
		ModeToggle,
		NotificationMenu,
		LanguageSelector
	} from '$lib/components/app'
	import * as Avatar from '$lib/components/ui/avatar'
	import { page } from '$app/state'
	import {
		PUBLIC_KEYCLOAK_CLIENT_ID,
		PUBLIC_KEYCLOAK_REALM,
		PUBLIC_KEYCLOAK_URL
	} from '$env/static/public'
	import { authClient } from '$lib/client'
	import { getAuthLocaleAuthorizationParameters } from '$lib/auth-locale'
	import type { Attachment } from 'svelte/attachments'
	import AppContainer from './app-container.svelte'
	import { getLocale } from '$lib/paraglide/runtime'
	import * as m from '$lib/paraglide/messages'
	import { resolve } from '$app/paths'
	import type {
		AdministrationAllowedActionsDto,
		PlatformAllowedActionsDto
	} from '$lib/utils/client'

	const session = authClient.useSession()
	let {
		platformAllowedActions,
		administrationAllowedActions
	}: {
		platformAllowedActions: PlatformAllowedActionsDto
		administrationAllowedActions: AdministrationAllowedActionsDto
	} = $props()

	const permissions = $derived.by(() => {
		const user = $session.data?.user
		return {
			canCreateForum: platformAllowedActions.canManageStructure,
			canCreateCategory: platformAllowedActions.canManageStructure,
			canCreateThread: user !== undefined
		}
	})

	const syncAppBarHeight: Attachment<HTMLElement> = (element) => {
		const updateHeight = () => {
			document.documentElement.style.setProperty(
				'--app-bar-height',
				`${element.clientHeight + 8}px`
			)
		}
		const observer = new ResizeObserver(updateHeight)

		observer.observe(element)
		updateHeight()

		return () => {
			observer.disconnect()
			document.documentElement.style.removeProperty('--app-bar-height')
		}
	}

	async function signOut() {
		let idToken: string | undefined

		try {
			idToken = (await authClient.getAccessToken({ useAccountCookie: true })).data?.idToken
		} catch {
			// Continue with local sign-out even when the Keycloak ID token is unavailable.
		}

		await authClient.signOut()

		const keycloakLogoutUrl = new URL(
			`${PUBLIC_KEYCLOAK_URL}/realms/${PUBLIC_KEYCLOAK_REALM}/protocol/openid-connect/logout`
		)
		keycloakLogoutUrl.searchParams.set('client_id', PUBLIC_KEYCLOAK_CLIENT_ID)
		keycloakLogoutUrl.searchParams.set(
			'post_logout_redirect_uri',
			new URL(resolve('/'), window.location.origin).toString()
		)
		if (idToken) keycloakLogoutUrl.searchParams.set('id_token_hint', idToken)

		window.location.assign(keycloakLogoutUrl)
	}

	async function signIn() {
		try {
			const result = await authClient.signIn.social({
				provider: 'keycloak',
				callbackURL: resolve('/'),
				errorCallbackURL: resolve('/(app)/auth/error'),
				additionalParams: getAuthLocaleAuthorizationParameters(getLocale())
			})

			if (!result.error) return
		} catch {
			// The localized error page provides a stable fallback for network failures.
		}

		window.location.assign(resolve('/(app)/auth/error'))
	}
</script>

<header
	{@attach syncAppBarHeight}
	class="sticky top-0 z-50 w-full border-b border-border/40 bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60"
>
	<AppContainer class="flex h-14 items-center">
		<MainNav {administrationAllowedActions} />
		<MobileNav {administrationAllowedActions} />
		<div class="ml-auto flex min-w-0 flex-1 items-center justify-end gap-x-2 md:gap-x-4">
			{#if page.url.pathname !== resolve('/(app)/search')}
				<ForumSearch />
			{/if}
			<nav class="flex items-center gap-x-2" aria-label={m.nav_account()}>
				<DropdownMenu.Root>
					<DropdownMenu.Trigger>
						{#snippet child({ props })}
							<Button {...props} variant="outline" size="icon" class="relative size-8 rounded-full">
								{#if $session.data}
									<Avatar.Root class="size-8">
										<Avatar.Image
											src={$session.data.user.avatarUrl}
											alt={$session.data.user.name}
										/>
										<Avatar.Fallback>{$session.data.user.name}</Avatar.Fallback>
									</Avatar.Root>
								{:else}
									<UserCircleIcon />
								{/if}
								<span class="sr-only">{m.auth_menu_toggle()}</span>
							</Button>
						{/snippet}
					</DropdownMenu.Trigger>
					<DropdownMenu.Content align="end">
						<DropdownMenu.Group>
							{#if $session.data}
								<DropdownMenu.GroupHeading
									><div class="flex flex-col gap-1">
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
										{#snippet child({ props })}
											<a {...props} href={resolve('/(app)/forums/create')}>
												<FolderPlusIcon />
												{m.forum_create()}
											</a>
										{/snippet}
									</DropdownMenu.Item>
								{/if}
								{#if permissions.canCreateCategory}
									<DropdownMenu.Item>
										{#snippet child({ props })}
											<a {...props} href={resolve('/(app)/categories/create')}>
												<CirclePlusIcon />
												{m.category_create()}
											</a>
										{/snippet}
									</DropdownMenu.Item>
								{/if}
								{#if permissions.canCreateThread}
									<DropdownMenu.Item>
										{#snippet child({ props })}
											<a {...props} href={resolve('/(app)/threads/create')}>
												<FilePlus2Icon />
												{m.thread_create()}
											</a>
										{/snippet}
									</DropdownMenu.Item>
								{/if}
								<DropdownMenu.Separator />
								<DropdownMenu.Item>
									{#snippet child({ props })}
										<a {...props} href={resolve('/(app)/current-user/thread-drafts')}>
											<PencilIcon />
											{m.nav_thread_drafts()}
										</a>
									{/snippet}
								</DropdownMenu.Item>
								<DropdownMenu.Separator />
								<DropdownMenu.Item>
									{#snippet child({ props })}
										<a {...props} href={resolve('/(app)/settings/profile')}>
											<SettingsIcon />
											{m.nav_settings()}
										</a>
									{/snippet}
								</DropdownMenu.Item>
								<DropdownMenu.Separator />
								<DropdownMenu.Item
									onclick={async () => {
										await signOut()
									}}><LogOutIcon />{m.auth_logout()}</DropdownMenu.Item
								>
							{:else}
								<DropdownMenu.Item onclick={signIn}>
									<LogInIcon />{m.auth_login()}</DropdownMenu.Item
								>
							{/if}
						</DropdownMenu.Group>
					</DropdownMenu.Content>
				</DropdownMenu.Root>
			</nav>
			<div class="flex items-center gap-x-2">
				<NotificationMenu />
				<LanguageSelector />
				<ModeToggle />
			</div>
		</div>
	</AppContainer>
</header>

<style>
	:global(html) {
		scroll-padding-top: var(--app-bar-height, 0px);
	}
</style>
