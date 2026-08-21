<script lang="ts">
	import UserRoundCogIcon from '@lucide/svelte/icons/user-round-cog'
	import UserRoundPlusIcon from '@lucide/svelte/icons/user-round-plus'
	import UserRoundXIcon from '@lucide/svelte/icons/user-round-x'
	import type { AuthorizationScopeSelection } from '$lib/authorization-scope'
	import { withApiLocale } from '$lib/client/api-options'
	import { Button } from '$lib/components/ui/button'
	import * as Card from '$lib/components/ui/card'
	import * as Empty from '$lib/components/ui/empty'
	import * as Field from '$lib/components/ui/field'
	import { Input } from '$lib/components/ui/input'
	import * as Item from '$lib/components/ui/item'
	import { Spinner } from '$lib/components/ui/spinner'
	import * as m from '$lib/paraglide/messages'
	import {
		AuthorizationScopeType,
		appointCategoryModerator,
		appointForumModerator,
		getCategoryModerators,
		getForumModerators,
		getUsersBulk,
		getUsersPaged,
		revokeCategoryModerator,
		revokeForumModerator,
		type AuthorizationAssignmentId,
		type CategoryModeratorAppointmentDto,
		type ForumModeratorAppointmentDto,
		type UserDto,
		type UserId
	} from '$lib/utils/client'
	import { formatTimestamp } from '$lib/utils/format'
	import { getSuccessfulResultMap } from '$lib/utils/result'
	import { parseUsernameSearchTerm } from '$lib/utils/value-object'
	import AuthorizationScopeSelector from './authorization-scope-selector.svelte'
	import ConfirmationDialog from './confirmation-dialog.svelte'
	import RemoteCombobox from './remote-combobox.svelte'

	type ModeratorAppointment = CategoryModeratorAppointmentDto | ForumModeratorAppointmentDto

	let selectedScope = $state<AuthorizationScopeSelection>()
	let appointments = $state.raw<ModeratorAppointment[]>([])
	let users = $state.raw<Map<UserId, UserDto>>(new Map())
	let selectedUserId = $state<UserId>()
	let validUntil = $state('')
	let validityError = $state<string>()
	let loadError = $state<string>()
	let mutationError = $state<string>()
	let loading = $state(false)
	let appointing = $state(false)
	let revokingAssignmentId = $state<AuthorizationAssignmentId>()
	let pendingRevokeAppointment = $state<ModeratorAppointment>()
	let revokeDialogOpen = $state(false)
	let loadSequence = 0

	const assignedUserIds = $derived(new Set(appointments.map((appointment) => appointment.userId)))
	const mutationInProgress = $derived(appointing || revokingAssignmentId !== undefined)

	async function loadUsers(query: string, signal: AbortSignal) {
		const username = parseUsernameSearchTerm(query)
		if (!username) return []

		const response = await getUsersPaged<true>(
			withApiLocale({ query: { username }, signal, throwOnError: true })
		)

		return response.data
			.filter((user) => user.enabled && !assignedUserIds.has(user.userId))
			.map((user) => ({ key: user.userId, value: { title: user.username } }))
	}

	async function loadAppointmentUsers(nextAppointments: ModeratorAppointment[]) {
		const userIds = new Set(
			nextAppointments.flatMap((appointment) => [appointment.userId, appointment.grantedBy])
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
		appointments = []
		users = new Map()
		loadError = undefined
		mutationError = undefined
		loading = false
		const sequence = ++loadSequence
		if (!nextScope) return

		loading = true
		try {
			let nextAppointments: ModeratorAppointment[]
			if (nextScope.scopeType === AuthorizationScopeType.FORUM) {
				nextAppointments = (
					await getForumModerators<true>(
						withApiLocale({ path: { forumId: nextScope.forumId }, throwOnError: true })
					)
				).data
			} else if (nextScope.scopeType === AuthorizationScopeType.CATEGORY) {
				nextAppointments = (
					await getCategoryModerators<true>(
						withApiLocale({ path: { categoryId: nextScope.categoryId }, throwOnError: true })
					)
				).data
			} else {
				return
			}

			const nextUsers = await loadAppointmentUsers(nextAppointments)
			if (sequence !== loadSequence) return
			appointments = nextAppointments
			users = nextUsers
		} catch (error) {
			if (sequence !== loadSequence) return
			console.error('Failed to load moderator appointments:', error)
			loadError = m.moderators_load_error()
		} finally {
			if (sequence === loadSequence) loading = false
		}
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

	async function appointModerator(event: SubmitEvent) {
		event.preventDefault()
		if (!selectedScope || !selectedUserId || mutationInProgress) return

		const expiration = parseExpiration()
		if (expiration === undefined) return

		mutationError = undefined
		appointing = true
		try {
			let result
			if (selectedScope.scopeType === AuthorizationScopeType.FORUM) {
				result = await appointForumModerator<false>(
					withApiLocale({
						path: { forumId: selectedScope.forumId, userId: selectedUserId },
						query: expiration ? { validUntil: expiration } : undefined,
						throwOnError: false
					})
				)
			} else if (selectedScope.scopeType === AuthorizationScopeType.CATEGORY) {
				result = await appointCategoryModerator<false>(
					withApiLocale({
						path: { categoryId: selectedScope.categoryId, userId: selectedUserId },
						query: expiration ? { validUntil: expiration } : undefined,
						throwOnError: false
					})
				)
			} else {
				return
			}

			if (result.error) {
				mutationError =
					result.response?.status === 409
						? m.moderator_duplicate_error()
						: m.moderator_appoint_error()
				return
			}

			selectedUserId = undefined
			validUntil = ''
			await selectScope(selectedScope)
		} catch (error) {
			console.error('Failed to appoint moderator:', error)
			mutationError = m.moderator_appoint_error()
		} finally {
			appointing = false
		}
	}

	async function revokeModerator(appointment: ModeratorAppointment) {
		if (!selectedScope || mutationInProgress) return

		mutationError = undefined
		revokingAssignmentId = appointment.assignmentId
		try {
			let result
			if (selectedScope.scopeType === AuthorizationScopeType.FORUM) {
				result = await revokeForumModerator<false>(
					withApiLocale({
						path: { forumId: selectedScope.forumId, userId: appointment.userId },
						throwOnError: false
					})
				)
			} else if (selectedScope.scopeType === AuthorizationScopeType.CATEGORY) {
				result = await revokeCategoryModerator<false>(
					withApiLocale({
						path: { categoryId: selectedScope.categoryId, userId: appointment.userId },
						throwOnError: false
					})
				)
			} else {
				return
			}

			if (result.error) {
				mutationError = m.moderator_revoke_error()
				return
			}

			await selectScope(selectedScope)
		} catch (error) {
			console.error('Failed to revoke moderator:', error)
			mutationError = m.moderator_revoke_error()
		} finally {
			revokingAssignmentId = undefined
			pendingRevokeAppointment = undefined
			revokeDialogOpen = false
		}
	}
</script>

<Card.Root>
	<Card.Header>
		<Card.Title>{m.moderators()}</Card.Title>
		<Card.Description>{m.moderators_description()}</Card.Description>
	</Card.Header>
	<Card.Content class="flex flex-col gap-6">
		<form onsubmit={appointModerator}>
			<Field.Group>
				<AuthorizationScopeSelector
					allowedScopes={[AuthorizationScopeType.FORUM, AuthorizationScopeType.CATEGORY]}
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
				<Field.Field data-invalid={validityError !== undefined}>
					<Field.Label for="moderator-valid-until">{m.authorization_valid_until()}</Field.Label>
					<Input
						id="moderator-valid-until"
						type="datetime-local"
						bind:value={validUntil}
						aria-invalid={validityError !== undefined}
					/>
					<Field.Description>{m.authorization_valid_until_description()}</Field.Description>
					{#if validityError}<Field.Error>{validityError}</Field.Error>{/if}
				</Field.Field>
				<Field.Field>
					<Button type="submit" disabled={!selectedScope || !selectedUserId || mutationInProgress}>
						{#if appointing}
							<Spinner data-icon="inline-start" />
						{:else}
							<UserRoundPlusIcon data-icon="inline-start" />
						{/if}
						{m.moderator_appoint()}
					</Button>
					{#if mutationError}<Field.Error>{mutationError}</Field.Error>{/if}
				</Field.Field>
			</Field.Group>
		</form>

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
					<Empty.Media variant="icon"><UserRoundCogIcon /></Empty.Media>
					<Empty.Title>{m.authorization_scope_select()}</Empty.Title>
					<Empty.Description>{m.moderators_scope_description()}</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{:else if appointments.length === 0}
			<Empty.Root>
				<Empty.Header>
					<Empty.Media variant="icon"><UserRoundCogIcon /></Empty.Media>
					<Empty.Title>{m.moderators_none()}</Empty.Title>
					<Empty.Description>{m.moderators_none_description()}</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{:else}
			<Item.Group>
				{#each appointments as appointment (appointment.assignmentId)}
					<Item.Root variant="outline">
						<Item.Content>
							<Item.Title>{users.get(appointment.userId)?.username ?? m.user_unknown()}</Item.Title>
							<Item.Description>
								{m.moderator_appointed_by({
									date: formatTimestamp(appointment.grantedAt),
									user: users.get(appointment.grantedBy)?.username ?? m.user_unknown()
								})}
								{#if appointment.validUntil}
									· {m.authorization_expires({ date: formatTimestamp(appointment.validUntil) })}
								{/if}
							</Item.Description>
						</Item.Content>
						<Item.Actions>
							<Button
								type="button"
								variant="destructive"
								size="sm"
								disabled={mutationInProgress}
								onclick={() => {
									pendingRevokeAppointment = appointment
									revokeDialogOpen = true
								}}
							>
								{#if revokingAssignmentId === appointment.assignmentId}
									<Spinner data-icon="inline-start" />
								{:else}
									<UserRoundXIcon data-icon="inline-start" />
								{/if}
								{m.common_revoke()}
							</Button>
						</Item.Actions>
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
	busy={revokingAssignmentId !== undefined}
	onConfirm={() => pendingRevokeAppointment && revokeModerator(pendingRevokeAppointment)}
/>
