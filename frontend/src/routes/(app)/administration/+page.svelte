<script lang="ts">
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import {
		CapabilityGrantManager,
		ForumSanctionManager,
		ModeratorAssignmentManager,
		PlatformAdministratorManager
	} from '$lib/components/app'
	import * as Tabs from '$lib/components/ui/tabs'
	import * as m from '$lib/paraglide/messages'
	import { AuthorizationScopeType } from '$lib/utils/client'
	import type { PageProps } from './$types'

	let { data }: PageProps = $props()
	const defaultTab = $derived(
		data.allowedActions.canManagePlatformAuthorization
			? 'administrators'
			: data.allowedActions.canManageAnyAuthorization
				? 'moderators'
				: 'sanctions'
	)
	const nonPlatformScopes = [
		AuthorizationScopeType.FORUM,
		AuthorizationScopeType.CATEGORY,
		AuthorizationScopeType.THREAD
	] as const
	const authorizationScopes = $derived(
		data.allowedActions.canManagePlatformAuthorization
			? [AuthorizationScopeType.PLATFORM, ...nonPlatformScopes]
			: nonPlatformScopes
	)
	const sanctionScopes = $derived(
		data.allowedActions.canManagePlatformSanctions
			? [AuthorizationScopeType.PLATFORM, ...nonPlatformScopes]
			: nonPlatformScopes
	)
</script>

<svelte:head>
	<title>{m.administration_title()} — {PUBLIC_APP_NAME}</title>
</svelte:head>

<section class="flex flex-col gap-6">
	<header class="flex flex-col gap-2">
		<h1 class="text-xl font-bold sm:text-2xl">{m.administration_title()}</h1>
		<p class="text-sm text-muted-foreground">{m.administration_description()}</p>
	</header>

	<Tabs.Root value={defaultTab}>
		<Tabs.List variant="line" class="h-auto flex-wrap justify-start gap-y-1">
			{#if data.allowedActions.canManagePlatformAuthorization}
				<Tabs.Trigger value="administrators">{m.administration_administrators()}</Tabs.Trigger>
			{/if}
			{#if data.allowedActions.canManageAnyAuthorization}
				<Tabs.Trigger value="moderators">{m.administration_moderators()}</Tabs.Trigger>
				<Tabs.Trigger value="capabilities">{m.administration_capabilities()}</Tabs.Trigger>
			{/if}
			{#if data.allowedActions.canManageAnySanctions}
				<Tabs.Trigger value="sanctions">{m.administration_sanctions()}</Tabs.Trigger>
			{/if}
		</Tabs.List>
		{#if data.allowedActions.canManagePlatformAuthorization}
			<Tabs.Content value="administrators" class="pt-4">
				<PlatformAdministratorManager appointments={data.appointments} users={data.users} />
			</Tabs.Content>
		{/if}
		{#if data.allowedActions.canManageAnyAuthorization}
			<Tabs.Content value="moderators" class="pt-4">
				<ModeratorAssignmentManager />
			</Tabs.Content>
			<Tabs.Content value="capabilities" class="pt-4">
				<CapabilityGrantManager allowedScopes={authorizationScopes} />
			</Tabs.Content>
		{/if}
		{#if data.allowedActions.canManageAnySanctions}
			<Tabs.Content value="sanctions" class="pt-4">
				<ForumSanctionManager allowedScopes={sanctionScopes} />
			</Tabs.Content>
		{/if}
	</Tabs.Root>
</section>
