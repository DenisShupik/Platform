<script lang="ts">
	import { invalidateAll } from '$app/navigation'
	import ShieldUserIcon from '@lucide/svelte/icons/shield-user'
	import UserRoundPlusIcon from '@lucide/svelte/icons/user-round-plus'
	import UserRoundXIcon from '@lucide/svelte/icons/user-round-x'
	import { withApiLocale } from '$lib/client/api-options'
	import * as Card from '$lib/components/ui/card'
	import * as Empty from '$lib/components/ui/empty'
	import * as Field from '$lib/components/ui/field'
	import { Button } from '$lib/components/ui/button'
	import * as Item from '$lib/components/ui/item'
	import { Spinner } from '$lib/components/ui/spinner'
	import * as m from '$lib/paraglide/messages'
	import {
		appointPlatformAdministrator,
		getUsersPaged,
		revokePlatformAdministrator,
		type PlatformAdministratorAppointmentDto,
		type UserDto,
		type UserId
	} from '$lib/utils/client'
	import { formatTimestamp } from '$lib/utils/format'
	import { parseUsernameSearchTerm } from '$lib/utils/value-object'
	import RemoteCombobox from './remote-combobox.svelte'
	import ConfirmationDialog from './confirmation-dialog.svelte'

	let {
		appointments,
		users
	}: {
		appointments: PlatformAdministratorAppointmentDto[]
		users: Map<UserId, UserDto>
	} = $props()

	let selectedUserId = $state<UserId>()
	let mutationError = $state<string>()
	let appointing = $state(false)
	let revokingUserId = $state<UserId>()
	let pendingRevokeUserId = $state<UserId>()
	let revokeDialogOpen = $state(false)

	const assignedUserIds = $derived(new Set(appointments.map((appointment) => appointment.userId)))
	const reachableAdministratorCount = $derived(
		appointments.filter((appointment) => users.get(appointment.userId)?.enabled === true).length
	)
	const mutationInProgress = $derived(appointing || revokingUserId !== undefined)

	async function loadUsers(query: string, signal: AbortSignal) {
		const username = parseUsernameSearchTerm(query)
		if (username === undefined) return []

		const response = await getUsersPaged<true>(
			withApiLocale({ query: { username }, signal, throwOnError: true })
		)

		return response.data
			.filter((user) => user.enabled && !assignedUserIds.has(user.userId))
			.map((user) => ({ key: user.userId, value: { title: user.username } }))
	}

	async function appointAdministrator(event: SubmitEvent) {
		event.preventDefault()
		if (!selectedUserId || mutationInProgress) return

		mutationError = undefined
		appointing = true
		try {
			const result = await appointPlatformAdministrator<false>(
				withApiLocale({
					path: { userId: selectedUserId },
					throwOnError: false
				})
			)

			if (result.error) {
				mutationError =
					result.response?.status === 409
						? m.platform_administrator_duplicate_error()
						: m.platform_administrator_appoint_error()
				return
			}

			selectedUserId = undefined
			await invalidateAll()
		} catch (error) {
			console.error('Failed to appoint platform administrator:', error)
			mutationError = m.platform_administrator_appoint_error()
		} finally {
			appointing = false
		}
	}

	async function revokeAdministrator(userId: UserId) {
		if (
			mutationInProgress ||
			(users.get(userId)?.enabled === true && reachableAdministratorCount <= 1)
		)
			return

		mutationError = undefined
		revokingUserId = userId
		try {
			const result = await revokePlatformAdministrator<false>(
				withApiLocale({ path: { userId }, throwOnError: false })
			)

			if (result.error) {
				mutationError =
					result.response?.status === 409
						? m.platform_administrator_last_error()
						: m.platform_administrator_revoke_error()
				return
			}

			await invalidateAll()
		} catch (error) {
			console.error('Failed to revoke platform administrator:', error)
			mutationError = m.platform_administrator_revoke_error()
		} finally {
			revokingUserId = undefined
			pendingRevokeUserId = undefined
			revokeDialogOpen = false
		}
	}
</script>

<Card.Root>
	<Card.Header>
		<Card.Title>{m.platform_administrators()}</Card.Title>
		<Card.Description>{m.platform_administrators_description()}</Card.Description>
	</Card.Header>
	<Card.Content class="flex flex-col gap-6">
		<form onsubmit={appointAdministrator}>
			<Field.Group>
				<Field.Field>
					<RemoteCombobox
						bind:value={selectedUserId}
						label={m.platform_administrator_user()}
						placeholder={m.user_select()}
						searchPlaceholder={m.user_search()}
						emptyText={m.user_none()}
						standalone
						initialOptions={[]}
						loadOptions={loadUsers}
					/>
				</Field.Field>
				<Field.Field>
					<Button type="submit" disabled={!selectedUserId || mutationInProgress}>
						{#if appointing}
							<Spinner data-icon="inline-start" />
						{:else}
							<UserRoundPlusIcon data-icon="inline-start" />
						{/if}
						{m.platform_administrator_appoint()}
					</Button>
					{#if mutationError}
						<Field.Error>{mutationError}</Field.Error>
					{/if}
				</Field.Field>
			</Field.Group>
		</form>

		{#if appointments.length === 0}
			<Empty.Root>
				<Empty.Header>
					<Empty.Media variant="icon"><ShieldUserIcon /></Empty.Media>
					<Empty.Title>{m.platform_administrators_none()}</Empty.Title>
					<Empty.Description>{m.platform_administrators_none_description()}</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{:else}
			<Item.Group>
				{#each appointments as appointment (appointment.assignmentId)}
					<Item.Root variant="outline">
						<Item.Content>
							<Item.Title>
								{users.get(appointment.userId)?.username ?? m.user_unknown()}
							</Item.Title>
							<Item.Description>
								{#if appointment.wasBootstrapped}
									{m.platform_administrator_bootstrapped({
										date: formatTimestamp(appointment.grantedAt)
									})}
								{:else}
									{m.platform_administrator_appointed_by({
										date: formatTimestamp(appointment.grantedAt),
										user: appointment.grantedBy
											? (users.get(appointment.grantedBy)?.username ?? m.user_unknown())
											: m.user_unknown()
									})}
								{/if}
							</Item.Description>
						</Item.Content>
						<Item.Actions>
							<Button
								type="button"
								variant="destructive"
								size="sm"
								disabled={mutationInProgress ||
									(users.get(appointment.userId)?.enabled === true &&
										reachableAdministratorCount <= 1)}
								onclick={() => {
									pendingRevokeUserId = appointment.userId
									revokeDialogOpen = true
								}}
							>
								{#if revokingUserId === appointment.userId}
									<Spinner data-icon="inline-start" />
								{:else}
									<UserRoundXIcon data-icon="inline-start" />
								{/if}
								{m.platform_administrator_revoke()}
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
	confirmLabel={m.platform_administrator_revoke()}
	busy={revokingUserId !== undefined}
	onConfirm={() => pendingRevokeUserId && revokeAdministrator(pendingRevokeUserId)}
/>
