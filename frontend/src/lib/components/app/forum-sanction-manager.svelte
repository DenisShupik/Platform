<script lang="ts">
	import PlusIcon from '@lucide/svelte/icons/plus'
	import ShieldAlertIcon from '@lucide/svelte/icons/shield-alert'
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
	import { Textarea } from '$lib/components/ui/textarea'
	import * as m from '$lib/paraglide/messages'
	import {
		AuthorizationScopeType,
		ForumSanctionType,
		getForumSanctions,
		getUsersBulk,
		getUsersPaged,
		issueForumSanction,
		revokeForumSanction,
		type ForumSanctionDto,
		type ForumSanctionId,
		type UserDto,
		type UserId
	} from '$lib/utils/client'
	import { formatTimestamp } from '$lib/utils/format'
	import { getSuccessfulResultMap } from '$lib/utils/result'
	import { parseForumSanctionReason, parseUsernameSearchTerm } from '$lib/utils/value-object'
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

	const sanctionTypeOptions = [
		{ value: ForumSanctionType.READ_ONLY, label: m.sanction_read_only() },
		{ value: ForumSanctionType.NO_ACCESS, label: m.sanction_no_access() }
	]

	let selectedScope = $state<AuthorizationScopeSelection | undefined>(
		untrack(() =>
			allowedScopes.includes(AuthorizationScopeType.PLATFORM)
				? platformAuthorizationScope
				: undefined
		)
	)
	let sanctions = $state.raw<ForumSanctionDto[]>([])
	let users = $state.raw<Map<UserId, UserDto>>(new Map())
	let selectedUserId = $state<UserId>()
	let selectedType = $state<ForumSanctionType>(ForumSanctionType.READ_ONLY)
	let reason = $state('')
	let validUntil = $state('')
	let reasonError = $state<string>()
	let validityError = $state<string>()
	let loadError = $state<string>()
	let mutationError = $state<string>()
	let loading = $state(false)
	let showHistory = $state(false)
	let issuing = $state(false)
	let revokingSanctionId = $state<ForumSanctionId>()
	let pendingRevokeSanctionId = $state<ForumSanctionId>()
	let revokeDialogOpen = $state(false)
	let loadSequence = 0

	const activeSanctions = $derived(
		sanctions.filter(
			(sanction) =>
				sanction.revokedAt === null &&
				(sanction.validUntil === null || sanction.validUntil > new Date())
		)
	)
	const assignedKeys = $derived(
		new Set(activeSanctions.map((sanction) => `${sanction.userId}:${sanction.type}`))
	)
	const mutationInProgress = $derived(issuing || revokingSanctionId !== undefined)
	const selectedTypeLabel = $derived(
		sanctionTypeOptions.find((option) => option.value === selectedType)?.label ?? ''
	)

	onMount(() => {
		if (allowedScopes.includes(AuthorizationScopeType.PLATFORM)) {
			void selectScope(platformAuthorizationScope)
		}
	})

	function sanctionTypeLabel(type: ForumSanctionType) {
		return sanctionTypeOptions.find((option) => option.value === type)?.label ?? type
	}

	function isActive(sanction: ForumSanctionDto) {
		return (
			sanction.revokedAt === null &&
			(sanction.validUntil === null || sanction.validUntil > new Date())
		)
	}

	async function loadUsers(query: string, signal: AbortSignal) {
		const username = parseUsernameSearchTerm(query)
		if (!username) return []

		const response = await getUsersPaged<true>(
			withApiLocale({ query: { username }, signal, throwOnError: true })
		)
		return response.data
			.filter((user) => user.enabled && !assignedKeys.has(`${user.userId}:${selectedType}`))
			.map((user) => ({ key: user.userId, value: { title: user.username } }))
	}

	async function loadSanctionUsers(nextSanctions: ForumSanctionDto[]) {
		const userIds = new Set(
			nextSanctions.flatMap((sanction) => [
				sanction.userId,
				sanction.issuedBy,
				...(sanction.revokedBy ? [sanction.revokedBy] : [])
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
		sanctions = []
		users = new Map()
		loadError = undefined
		mutationError = undefined
		loading = false
		const sequence = ++loadSequence
		if (!nextScope) return

		loading = true
		try {
			const nextSanctions = (
				await getForumSanctions<true>(
					withApiLocale({
						query: { ...toAuthorizationScopeQuery(nextScope), includeInactive: showHistory },
						throwOnError: true
					})
				)
			).data
			const nextUsers = await loadSanctionUsers(nextSanctions)
			if (sequence !== loadSequence) return
			sanctions = nextSanctions
			users = nextUsers
		} catch (error) {
			if (sequence !== loadSequence) return
			console.error('Failed to load forum sanctions:', error)
			loadError = m.sanctions_load_error()
		} finally {
			if (sequence === loadSequence) loading = false
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

	async function issueSanction(event: SubmitEvent) {
		event.preventDefault()
		const scope = selectedScope
		if (!scope || !selectedUserId || mutationInProgress) return

		reasonError = undefined
		const parsedReason = parseForumSanctionReason(reason)
		if (!parsedReason) {
			reasonError = m.sanction_reason_error()
			return
		}

		const expiration = parseExpiration()
		if (expiration === undefined) return

		mutationError = undefined
		issuing = true
		try {
			const result = await issueForumSanction<false>(
				withApiLocale({
					body: {
						userId: selectedUserId,
						type: selectedType,
						...toAuthorizationScopeBody(scope),
						reason: parsedReason,
						validUntil: expiration
					},
					throwOnError: false
				})
			)

			if (result.error) {
				mutationError =
					result.response?.status === 409 ? m.sanction_duplicate_error() : m.sanction_issue_error()
				return
			}

			selectedUserId = undefined
			reason = ''
			validUntil = ''
			await selectScope(scope)
		} catch (error) {
			console.error('Failed to issue forum sanction:', error)
			mutationError = m.sanction_issue_error()
		} finally {
			issuing = false
		}
	}

	async function revokeSanction(sanctionId: ForumSanctionId) {
		const scope = selectedScope
		if (!scope || mutationInProgress) return

		mutationError = undefined
		revokingSanctionId = sanctionId
		try {
			const result = await revokeForumSanction<false>(
				withApiLocale({ path: { forumSanctionId: sanctionId }, throwOnError: false })
			)
			if (result.error) {
				mutationError = m.sanction_revoke_error()
				return
			}

			await selectScope(scope)
		} catch (error) {
			console.error('Failed to revoke forum sanction:', error)
			mutationError = m.sanction_revoke_error()
		} finally {
			revokingSanctionId = undefined
			pendingRevokeSanctionId = undefined
			revokeDialogOpen = false
		}
	}
</script>

<Card.Root>
	<Card.Header>
		<Card.Title>{m.sanctions()}</Card.Title>
		<Card.Description>{m.sanctions_description()}</Card.Description>
	</Card.Header>
	<Card.Content class="flex flex-col gap-6">
		<form onsubmit={issueSanction}>
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
					/>
				</Field.Field>
				<Field.Field>
					<Field.Label for="sanction-type">{m.sanction_type()}</Field.Label>
					<Select.Root type="single" bind:value={selectedType}>
						<Select.Trigger id="sanction-type" class="w-full">{selectedTypeLabel}</Select.Trigger>
						<Select.Content>
							<Select.Group>
								{#each sanctionTypeOptions as option (option.value)}
									<Select.Item value={option.value} label={option.label} />
								{/each}
							</Select.Group>
						</Select.Content>
					</Select.Root>
				</Field.Field>
				<Field.Field data-invalid={reasonError !== undefined}>
					<Field.Label for="sanction-reason">{m.sanction_reason()}</Field.Label>
					<Textarea
						id="sanction-reason"
						bind:value={reason}
						minlength={3}
						maxlength={500}
						aria-invalid={reasonError !== undefined}
					/>
					<Field.Description>{m.sanction_reason_description()}</Field.Description>
					{#if reasonError}<Field.Error>{reasonError}</Field.Error>{/if}
				</Field.Field>
				<Field.Field data-invalid={validityError !== undefined}>
					<Field.Label for="sanction-valid-until">{m.authorization_valid_until()}</Field.Label>
					<Input
						id="sanction-valid-until"
						type="datetime-local"
						bind:value={validUntil}
						aria-invalid={validityError !== undefined}
					/>
					<Field.Description>{m.authorization_valid_until_description()}</Field.Description>
					{#if validityError}<Field.Error>{validityError}</Field.Error>{/if}
				</Field.Field>
				<Field.Field>
					<Button type="submit" disabled={!selectedScope || !selectedUserId || mutationInProgress}>
						{#if issuing}
							<Spinner data-icon="inline-start" />
						{:else}
							<PlusIcon data-icon="inline-start" />
						{/if}
						{m.sanction_issue()}
					</Button>
					{#if mutationError}<Field.Error>{mutationError}</Field.Error>{/if}
				</Field.Field>
			</Field.Group>
		</form>

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
					<Empty.Media variant="icon"><ShieldAlertIcon /></Empty.Media>
					<Empty.Title>{m.authorization_scope_select()}</Empty.Title>
					<Empty.Description>{m.sanctions_scope_description()}</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{:else if sanctions.length === 0}
			<Empty.Root>
				<Empty.Header>
					<Empty.Media variant="icon"><ShieldAlertIcon /></Empty.Media>
					<Empty.Title>{m.sanctions_none()}</Empty.Title>
					<Empty.Description>{m.sanctions_none_description()}</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{:else}
			<Item.Group>
				{#each sanctions as sanction (sanction.forumSanctionId)}
					<Item.Root variant="outline">
						<Item.Content>
							<Item.Title>
								{users.get(sanction.userId)?.username ?? m.user_unknown()} ·
								{sanctionTypeLabel(sanction.type)}
							</Item.Title>
							<Item.Description>{sanction.reason}</Item.Description>
							<Item.Description>
								{m.sanction_issued_by({
									date: formatTimestamp(sanction.issuedAt),
									user: users.get(sanction.issuedBy)?.username ?? m.user_unknown()
								})}
								{#if sanction.validUntil}
									· {m.authorization_expires({ date: formatTimestamp(sanction.validUntil) })}
								{/if}
								{#if sanction.revokedAt}
									· {m.authorization_revoked({ date: formatTimestamp(sanction.revokedAt) })}
								{:else if !isActive(sanction)}
									· {m.authorization_expired()}
								{/if}
							</Item.Description>
						</Item.Content>
						{#if isActive(sanction)}
							<Item.Actions>
								<Button
									type="button"
									variant="destructive"
									size="sm"
									disabled={mutationInProgress}
									onclick={() => {
										pendingRevokeSanctionId = sanction.forumSanctionId
										revokeDialogOpen = true
									}}
								>
									{#if revokingSanctionId === sanction.forumSanctionId}
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
	busy={revokingSanctionId !== undefined}
	onConfirm={() => pendingRevokeSanctionId && revokeSanction(pendingRevokeSanctionId)}
/>
