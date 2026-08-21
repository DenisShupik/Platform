<script lang="ts">
	import KeyRoundIcon from '@lucide/svelte/icons/key-round'
	import KeyRoundPlusIcon from '@lucide/svelte/icons/key-round'
	import Trash2Icon from '@lucide/svelte/icons/trash-2'
	import { onMount, untrack } from 'svelte'
	import {
		platformAuthorizationScope,
		toAuthorizationScopeBody,
		toAuthorizationScopeQuery,
		type AuthorizationScopeSelection
	} from '$lib/authorization-scope'
	import { withApiLocale } from '$lib/client/api-options'
	import { Button } from '$lib/components/ui/button'
	import * as Card from '$lib/components/ui/card'
	import * as Empty from '$lib/components/ui/empty'
	import * as Field from '$lib/components/ui/field'
	import { Input } from '$lib/components/ui/input'
	import * as Item from '$lib/components/ui/item'
	import * as Select from '$lib/components/ui/select'
	import { Spinner } from '$lib/components/ui/spinner'
	import * as m from '$lib/paraglide/messages'
	import {
		AuthorizationScopeType,
		CapabilityCode,
		GrantSourceType,
		getCapabilityCatalog,
		getCapabilityGrants,
		getEffectiveCapabilityGrants,
		getUsersBulk,
		getUsersPaged,
		grantCapability,
		revokeCapability,
		type CapabilityGrantDto,
		type CapabilityGrantId,
		type CapabilityDefinitionDto,
		type UserDto,
		type UserId
	} from '$lib/utils/client'
	import { formatTimestamp } from '$lib/utils/format'
	import { getSuccessfulResultMap } from '$lib/utils/result'
	import { parseUsernameSearchTerm } from '$lib/utils/value-object'
	import AuthorizationScopeSelector from './authorization-scope-selector.svelte'
	import ConfirmationDialog from './confirmation-dialog.svelte'
	import RemoteCombobox from './remote-combobox.svelte'

	const allScopes = [
		AuthorizationScopeType.PLATFORM,
		AuthorizationScopeType.FORUM,
		AuthorizationScopeType.CATEGORY,
		AuthorizationScopeType.THREAD
	] as const
	let { allowedScopes = allScopes }: { allowedScopes?: readonly AuthorizationScopeType[] } =
		$props()

	const capabilityOptions = [
		{ value: CapabilityCode.MANAGE_STRUCTURE, label: m.capability_manage_structure() },
		{
			value: CapabilityCode.VIEW_UNPUBLISHED_THREADS,
			label: m.capability_view_unpublished_threads()
		},
		{ value: CapabilityCode.APPROVE_THREADS, label: m.capability_approve_threads() },
		{ value: CapabilityCode.REJECT_THREADS, label: m.capability_reject_threads() },
		{ value: CapabilityCode.EDIT_ANY_POST, label: m.capability_edit_any_post() },
		{ value: CapabilityCode.DELETE_ANY_POST, label: m.capability_delete_any_post() },
		{ value: CapabilityCode.MANAGE_AUTHORIZATION, label: m.capability_manage_authorization() },
		{ value: CapabilityCode.MANAGE_SANCTIONS, label: m.capability_manage_sanctions() }
	]

	let selectedScope = $state<AuthorizationScopeSelection | undefined>(
		untrack(() =>
			allowedScopes.includes(AuthorizationScopeType.PLATFORM)
				? platformAuthorizationScope
				: undefined
		)
	)
	let capabilityDefinitions = $state.raw<CapabilityDefinitionDto[]>([])
	let grants = $state.raw<CapabilityGrantDto[]>([])
	let effectiveGrants = $state.raw<CapabilityGrantDto[]>([])
	let users = $state.raw<Map<UserId, UserDto>>(new Map())
	let selectedUserId = $state<UserId>()
	let selectedCapability = $state<CapabilityCode>(CapabilityCode.MANAGE_STRUCTURE)
	let validUntil = $state('')
	let validityError = $state<string>()
	let loadError = $state<string>()
	let mutationError = $state<string>()
	let loading = $state(false)
	let effectiveLoading = $state(false)
	let effectiveLoadError = $state<string>()
	let showHistory = $state(false)
	let granting = $state(false)
	let revokingGrantId = $state<CapabilityGrantId>()
	let pendingRevokeGrantId = $state<CapabilityGrantId>()
	let revokeDialogOpen = $state(false)
	let loadSequence = 0

	const activeGrants = $derived(
		grants.filter(
			(grant) =>
				grant.revokedAt === null && (grant.validUntil === null || grant.validUntil > new Date())
		)
	)
	const assignedKeys = $derived(
		new Set(activeGrants.map((grant) => `${grant.userId}:${grant.capability}`))
	)
	const mutationInProgress = $derived(granting || revokingGrantId !== undefined)
	const availableCapabilityOptions = $derived(
		selectedScope
			? capabilityOptions.filter((option) =>
					capabilityDefinitions.some(
						(definition) =>
							definition.capability === option.value &&
							definition.allowedScopes.includes(selectedScope!.scopeType)
					)
				)
			: []
	)
	const selectedCapabilityLabel = $derived(
		capabilityOptions.find((option) => option.value === selectedCapability)?.label ?? ''
	)

	onMount(() => {
		void initialize()
	})

	async function initialize() {
		try {
			capabilityDefinitions = (
				await getCapabilityCatalog<true>(withApiLocale({ throwOnError: true }))
			).data
		} catch (error) {
			console.error('Failed to load capability catalog:', error)
			loadError = m.capability_grants_load_error()
			return
		}

		if (allowedScopes.includes(AuthorizationScopeType.PLATFORM)) {
			await selectScope(platformAuthorizationScope)
		}
	}

	function capabilityLabel(capability: CapabilityCode) {
		return capabilityOptions.find((option) => option.value === capability)?.label ?? capability
	}

	function sourceLabel(source: GrantSourceType) {
		switch (source) {
			case GrantSourceType.DIRECT:
				return m.capability_source_direct()
			case GrantSourceType.CATEGORY_MODERATOR_APPOINTMENT:
				return m.capability_source_category_moderator()
			case GrantSourceType.FORUM_MODERATOR_APPOINTMENT:
				return m.capability_source_forum_moderator()
			case GrantSourceType.PLATFORM_ADMINISTRATOR_APPOINTMENT:
				return m.capability_source_platform_administrator()
			case GrantSourceType.PLATFORM_ADMINISTRATOR_BOOTSTRAP:
				return m.capability_source_platform_bootstrap()
		}

		return source
	}

	function isActive(grant: CapabilityGrantDto) {
		return grant.revokedAt === null && (grant.validUntil === null || grant.validUntil > new Date())
	}

	async function loadUsers(query: string, signal: AbortSignal) {
		const username = parseUsernameSearchTerm(query)
		if (!username) return []

		const response = await getUsersPaged<true>(
			withApiLocale({ query: { username }, signal, throwOnError: true })
		)
		return response.data
			.filter((user) => user.enabled && !assignedKeys.has(`${user.userId}:${selectedCapability}`))
			.map((user) => ({ key: user.userId, value: { title: user.username } }))
	}

	async function loadGrantUsers(nextGrants: CapabilityGrantDto[]) {
		const userIds = new Set(
			nextGrants.flatMap((grant) => [
				grant.userId,
				...(grant.grantedBy ? [grant.grantedBy] : []),
				...(grant.revokedBy ? [grant.revokedBy] : [])
			])
		)
		if (userIds.size === 0) return new Map<UserId, UserDto>()

		const response = await getUsersBulk<true>(
			withApiLocale({ path: { userIds: [...userIds] }, throwOnError: true })
		)
		return getSuccessfulResultMap(response.data)
	}

	async function selectScope(nextScope: AuthorizationScopeSelection | undefined) {
		selectedScope = nextScope
		selectedUserId = undefined
		grants = []
		effectiveGrants = []
		effectiveLoadError = undefined
		users = new Map()
		loadError = undefined
		mutationError = undefined
		loading = false
		const sequence = ++loadSequence
		if (!nextScope) return
		const availableCapabilities = capabilityDefinitions
			.filter((definition) => definition.allowedScopes.includes(nextScope.scopeType))
			.map((definition) => definition.capability)
		if (!availableCapabilities.includes(selectedCapability)) {
			selectedCapability = availableCapabilities[0] ?? CapabilityCode.MANAGE_AUTHORIZATION
		}

		loading = true
		try {
			const nextGrants = (
				await getCapabilityGrants<true>(
					withApiLocale({
						query: { ...toAuthorizationScopeQuery(nextScope), includeInactive: showHistory },
						throwOnError: true
					})
				)
			).data
			const nextUsers = await loadGrantUsers(nextGrants)
			if (sequence !== loadSequence) return
			grants = nextGrants
			users = nextUsers
		} catch (error) {
			if (sequence !== loadSequence) return
			console.error('Failed to load capability grants:', error)
			loadError = m.capability_grants_load_error()
		} finally {
			if (sequence === loadSequence) loading = false
		}
	}

	async function selectUser(userId: UserId) {
		selectedUserId = userId
		const scope = selectedScope
		effectiveGrants = []
		effectiveLoadError = undefined
		if (!scope) return

		effectiveLoading = true
		try {
			effectiveGrants = (
				await getEffectiveCapabilityGrants<true>(
					withApiLocale({
						path: { userId },
						query: toAuthorizationScopeQuery(scope),
						throwOnError: true
					})
				)
			).data
		} catch (error) {
			console.error('Failed to load effective capability grants:', error)
			effectiveLoadError = m.capability_effective_rights_load_error()
		} finally {
			effectiveLoading = false
		}
	}

	async function toggleHistory() {
		showHistory = !showHistory
		await selectScope(selectedScope)
	}

	function parseExpiration() {
		validityError = undefined
		if (!validUntil) return null

		const expiration = new Date(validUntil)
		if (Number.isNaN(expiration.getTime()) || expiration <= new Date()) {
			validityError = m.authorization_valid_until_error()
			return undefined
		}
		return expiration
	}

	async function createGrant(event: SubmitEvent) {
		event.preventDefault()
		const scope = selectedScope
		if (!scope || !selectedUserId || mutationInProgress) return

		const expiration = parseExpiration()
		if (expiration === undefined) return

		mutationError = undefined
		granting = true
		try {
			const result = await grantCapability<false>(
				withApiLocale({
					body: {
						userId: selectedUserId,
						capability: selectedCapability,
						...toAuthorizationScopeBody(scope),
						validUntil: expiration
					},
					throwOnError: false
				})
			)

			if (result.error) {
				mutationError =
					result.response?.status === 409
						? m.capability_grant_duplicate_error()
						: m.capability_grant_create_error()
				return
			}

			selectedUserId = undefined
			validUntil = ''
			await selectScope(scope)
		} catch (error) {
			console.error('Failed to grant capability:', error)
			mutationError = m.capability_grant_create_error()
		} finally {
			granting = false
		}
	}

	async function revokeGrant(grantId: CapabilityGrantId) {
		const scope = selectedScope
		if (!scope || mutationInProgress) return

		mutationError = undefined
		revokingGrantId = grantId
		try {
			const result = await revokeCapability<false>(
				withApiLocale({ path: { capabilityGrantId: grantId }, throwOnError: false })
			)
			if (result.error) {
				mutationError = m.capability_grant_revoke_error()
				return
			}

			await selectScope(scope)
		} catch (error) {
			console.error('Failed to revoke capability grant:', error)
			mutationError = m.capability_grant_revoke_error()
		} finally {
			revokingGrantId = undefined
			pendingRevokeGrantId = undefined
			revokeDialogOpen = false
		}
	}
</script>

<Card.Root>
	<Card.Header>
		<Card.Title>{m.capability_grants()}</Card.Title>
		<Card.Description>{m.capability_grants_description()}</Card.Description>
	</Card.Header>
	<Card.Content class="flex flex-col gap-6">
		<form onsubmit={createGrant}>
			<Field.Group>
				<AuthorizationScopeSelector
					{allowedScopes}
					initialScopeType={allowedScopes[0]}
					onSelectionChange={selectScope}
				/>
				<Field.Field>
					<RemoteCombobox
						bind:value={selectedUserId}
						label={m.user()}
						placeholder={m.user_select()}
						searchPlaceholder={m.user_search()}
						emptyText={m.user_none()}
						standalone
						initialOptions={[]}
						loadOptions={loadUsers}
						onValueChange={selectUser}
					/>
				</Field.Field>
				<Field.Field>
					<Field.Label for="capability-select">{m.capability()}</Field.Label>
					<Select.Root type="single" bind:value={selectedCapability}>
						<Select.Trigger id="capability-select" class="w-full">
							{selectedCapabilityLabel}
						</Select.Trigger>
						<Select.Content>
							<Select.Group>
								{#each availableCapabilityOptions as option (option.value)}
									<Select.Item value={option.value} label={option.label} />
								{/each}
							</Select.Group>
						</Select.Content>
					</Select.Root>
				</Field.Field>
				<Field.Field data-invalid={validityError !== undefined}>
					<Field.Label for="capability-valid-until">{m.authorization_valid_until()}</Field.Label>
					<Input
						id="capability-valid-until"
						type="datetime-local"
						bind:value={validUntil}
						aria-invalid={validityError !== undefined}
					/>
					<Field.Description>{m.authorization_valid_until_description()}</Field.Description>
					{#if validityError}<Field.Error>{validityError}</Field.Error>{/if}
				</Field.Field>
				<Field.Field>
					<Button type="submit" disabled={!selectedScope || !selectedUserId || mutationInProgress}>
						{#if granting}
							<Spinner data-icon="inline-start" />
						{:else}
							<KeyRoundPlusIcon data-icon="inline-start" />
						{/if}
						{m.capability_grant_create()}
					</Button>
					{#if mutationError}<Field.Error>{mutationError}</Field.Error>{/if}
				</Field.Field>
			</Field.Group>
		</form>

		{#if selectedUserId}
			<section class="flex flex-col gap-3" aria-labelledby="effective-capabilities-title">
				<div>
					<h3 id="effective-capabilities-title" class="font-medium">
						{m.capability_effective_rights()}
					</h3>
					<p class="text-sm text-muted-foreground">
						{m.capability_effective_rights_description()}
					</p>
				</div>
				{#if effectiveLoading}
					<Spinner />
				{:else if effectiveLoadError}
					<Field.Error>{effectiveLoadError}</Field.Error>
				{:else if effectiveGrants.length === 0}
					<p class="text-sm text-muted-foreground">{m.capability_effective_rights_none()}</p>
				{:else}
					<Item.Group>
						{#each effectiveGrants as grant (grant.capabilityGrantId)}
							<Item.Root variant="outline">
								<Item.Content>
									<Item.Title>{capabilityLabel(grant.capability)}</Item.Title>
									<Item.Description>{sourceLabel(grant.sourceType)}</Item.Description>
								</Item.Content>
							</Item.Root>
						{/each}
					</Item.Group>
				{/if}
			</section>
		{/if}

		<div>
			<Button type="button" variant="outline" size="sm" onclick={toggleHistory} disabled={loading}>
				{showHistory ? m.authorization_history_hide() : m.authorization_history_show()}
			</Button>
		</div>

		{#if loading}
			<div class="flex items-center justify-center gap-2 py-8" aria-live="polite">
				<Spinner />
				<span class="text-sm text-muted-foreground">{m.common_loading()}</span>
			</div>
		{:else if loadError}
			<Field.Error>{loadError}</Field.Error>
		{:else if !selectedScope}
			<Empty.Root>
				<Empty.Header>
					<Empty.Media variant="icon"><KeyRoundIcon /></Empty.Media>
					<Empty.Title>{m.authorization_scope_select()}</Empty.Title>
					<Empty.Description>{m.capability_grants_scope_description()}</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{:else if grants.length === 0}
			<Empty.Root>
				<Empty.Header>
					<Empty.Media variant="icon"><KeyRoundIcon /></Empty.Media>
					<Empty.Title>{m.capability_grants_none()}</Empty.Title>
					<Empty.Description>{m.capability_grants_none_description()}</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{:else}
			<Item.Group>
				{#each grants as grant (grant.capabilityGrantId)}
					<Item.Root variant="outline">
						<Item.Content>
							<Item.Title>
								{users.get(grant.userId)?.username ?? m.user_unknown()} ·
								{capabilityLabel(grant.capability)}
							</Item.Title>
							<Item.Description>
								{m.capability_granted_by({
									date: formatTimestamp(grant.grantedAt),
									user: grant.grantedBy
										? (users.get(grant.grantedBy)?.username ?? m.user_unknown())
										: m.user_unknown()
								})}
								{#if grant.validUntil}
									· {m.authorization_expires({ date: formatTimestamp(grant.validUntil) })}
								{/if}
								{#if grant.revokedAt}
									· {m.authorization_revoked({ date: formatTimestamp(grant.revokedAt) })}
								{:else if !isActive(grant)}
									· {m.authorization_expired()}
								{/if}
							</Item.Description>
						</Item.Content>
						{#if isActive(grant)}
							<Item.Actions>
								<Button
									type="button"
									variant="destructive"
									size="sm"
									disabled={mutationInProgress}
									onclick={() => {
										pendingRevokeGrantId = grant.capabilityGrantId
										revokeDialogOpen = true
									}}
								>
									{#if revokingGrantId === grant.capabilityGrantId}
										<Spinner data-icon="inline-start" />
									{:else}
										<Trash2Icon data-icon="inline-start" />
									{/if}
									{m.common_revoke()}
								</Button>
							</Item.Actions>
						{/if}
					</Item.Root>
				{/each}
			</Item.Group>
		{/if}
	</Card.Content>
</Card.Root>

<ConfirmationDialog
	bind:open={revokeDialogOpen}
	title={m.authorization_revoke_confirm_title()}
	description={m.authorization_revoke_confirm_description()}
	confirmLabel={m.common_revoke()}
	busy={revokingGrantId !== undefined}
	onConfirm={() => pendingRevokeGrantId && revokeGrant(pendingRevokeGrantId)}
/>
