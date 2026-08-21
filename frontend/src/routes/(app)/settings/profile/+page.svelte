<script lang="ts">
	import { withApiLocale } from '$lib/client/api-options'
	import { browser } from '$app/environment'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import { authClient } from '$lib/client'
	import * as Alert from '$lib/components/ui/alert'
	import { Button } from '$lib/components/ui/button'
	import * as Card from '$lib/components/ui/card'
	import * as Field from '$lib/components/ui/field'
	import { Input } from '$lib/components/ui/input'
	import { Spinner } from '$lib/components/ui/spinner'
	import { convertToWebp } from '$lib/utils/convertToWebp'
	import { deleteAvatar, getUser, uploadAvatar, type UserId } from '$lib/utils/client'
	import CircleAlertIcon from '@lucide/svelte/icons/circle-alert'
	import type { Attachment } from 'svelte/attachments'
	import IconCamera from '~icons/tabler/camera'
	import IconPhotoX from '~icons/tabler/photo-x'
	import IconTrash from '~icons/tabler/trash'
	import * as m from '$lib/paraglide/messages'

	type ProfileFormData = {
		username: string
		email: string
	}

	let isUploading = $state(false)
	let isDeleting = $state(false)
	let avatarError = $state(false)
	let avatarActionError = $state<string>()

	const session = authClient.useSession()
	const userId = $derived($session.data?.user?.userId)
	const profilePromise = $derived(
		browser && userId !== undefined
			? loadProfile(userId, $session.data?.user.email ?? '')
			: undefined
	)

	async function loadProfile(profileUserId: UserId, email: string): Promise<ProfileFormData> {
		const result = await getUser<true>(
			withApiLocale({ path: { userId: profileUserId }, throwOnError: true })
		)
		return { username: result.data.username, email }
	}

	let fileInput = $state<HTMLInputElement>()
	const captureFileInput: Attachment<HTMLInputElement> = (element) => {
		fileInput = element

		return () => {
			if (fileInput === element) fileInput = undefined
		}
	}

	function handleClick() {
		fileInput?.click()
	}

	async function upload(
		event: Event & {
			currentTarget: EventTarget & HTMLInputElement
		}
	) {
		isUploading = true
		avatarActionError = undefined
		try {
			const files = event.currentTarget.files
			if (files == null) return
			if (files.length !== 1) return
			const file = files[0]
			if (file) {
				const blob = await convertToWebp(file)
				await uploadAvatar<true>(withApiLocale({ body: { file: blob }, throwOnError: true }))
				avatarError = false
				//if (currentUser.user !== undefined) setCurrentUserAvatarUrl(currentUser.user.id, true)
			}
		} catch (error) {
			console.error('Failed to upload avatar:', error)
			avatarActionError = m.profile_avatar_upload_error()
		} finally {
			event.currentTarget.value = ''
			isUploading = false
		}
	}

	async function handleDelete() {
		try {
			isDeleting = true
			avatarActionError = undefined
			await deleteAvatar<true>(withApiLocale({ throwOnError: true }))
			avatarError = true
			//if (currentUser.user !== undefined) setCurrentUserAvatarUrl(undefined)
		} catch (error) {
			console.error('Failed to delete avatar:', error)
			avatarActionError = m.profile_avatar_delete_error()
		} finally {
			isDeleting = false
		}
	}
</script>

<svelte:head>
	<title>{m.nav_profile()} — {PUBLIC_APP_NAME}</title>
</svelte:head>

<div class="grid grid-cols-1 gap-y-4 md:grid-cols-[auto_1fr] md:gap-4">
	{#if profilePromise}
		{#await profilePromise}
			<div
				class="col-span-full flex min-h-48 items-center justify-center"
				aria-label={m.profile_loading()}
			>
				<Spinner class="size-6" />
			</div>
		{:then formData}
			<Card.Root class="grid min-w-48">
				<Card.Header class="flex flex-col gap-1">
					<Card.Title class="text-2xl">{m.profile_avatar()}</Card.Title>
					<Card.Description>{m.profile_avatar_description()}</Card.Description>
				</Card.Header>
				<Card.Content>
					<div class="relative grid h-32 md:w-36 lg:w-64">
						{#if !avatarError}
							<img
								src={$session.data?.user?.avatarUrl}
								alt={$session.data?.user?.name}
								width="128"
								height="128"
								class="h-full max-h-32 w-full max-w-32 place-self-center rounded-lg border object-contain shadow-sm"
								onerror={() => {
									avatarError = true
								}}
							/>
						{:else}
							<div
								class="grid h-full max-h-32 w-full max-w-32 place-self-center rounded-lg border-2 border-dashed shadow-sm"
							>
								<IconPhotoX class="h-8 w-8 place-self-center text-muted" aria-hidden="true" />
							</div>
						{/if}
					</div>
					{#if avatarActionError}
						<Alert.Root variant="destructive" class="mt-4">
							<CircleAlertIcon aria-hidden="true" />
							<Alert.Title>{m.profile_avatar_failed()}</Alert.Title>
							<Alert.Description>{avatarActionError}</Alert.Description>
						</Alert.Root>
					{/if}
				</Card.Content>
				<Card.Footer class="grid w-44 grid-flow-col gap-x-4 place-self-center">
					<input
						type="file"
						class="hidden"
						accept="image/*"
						onchange={upload}
						{@attach captureFileInput}
					/>
					<Button
						onclick={handleClick}
						disabled={isUploading || isDeleting}
						aria-label={m.profile_avatar_upload()}
					>
						{#if isUploading}
							<Spinner class="size-6" />
						{:else}
							<IconCamera class="size-6" aria-hidden="true" />
						{/if}
					</Button>
					<Button
						variant="destructive"
						onclick={handleDelete}
						disabled={isUploading || isDeleting}
						aria-label={m.profile_avatar_delete()}
					>
						{#if isDeleting}
							<Spinner class="size-6" />
						{:else}
							<IconTrash class="size-6" aria-hidden="true" />
						{/if}</Button
					>
				</Card.Footer>
			</Card.Root>
			<Card.Root>
				<Card.Header class="flex flex-col gap-1">
					<Card.Title class="text-2xl">{m.profile_account()}</Card.Title>
					<Card.Description>{m.profile_account_description()}</Card.Description>
				</Card.Header>
				<Card.Content>
					<Field.FieldGroup>
						<Field.Field data-disabled="true">
							<Field.FieldLabel for="username">{m.profile_username()}</Field.FieldLabel>
							<Input type="text" id="username" value={formData.username} disabled />
						</Field.Field>
						<Field.Field data-disabled="true">
							<Field.FieldLabel for="email">{m.profile_email()}</Field.FieldLabel>
							<Input type="email" id="email" value={formData.email} disabled />
						</Field.Field>
					</Field.FieldGroup>
				</Card.Content>
				<Card.Footer />
			</Card.Root>
		{:catch}
			<Alert.Root variant="destructive" class="col-span-full">
				<CircleAlertIcon aria-hidden="true" />
				<Alert.Title>{m.profile_unavailable()}</Alert.Title>
				<Alert.Description>{m.profile_unavailable_description()}</Alert.Description>
			</Alert.Root>
		{/await}
	{/if}
</div>
