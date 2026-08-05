<script lang="ts">
	import { Button } from '$lib/components/ui/button'
	import { Input } from '$lib/components/ui/input'
	import * as Card from '$lib/components/ui/card'
	import * as Field from '$lib/components/ui/field'
	import IconCamera from '~icons/tabler/camera'
	import IconTrash from '~icons/tabler/trash'
	import { Spinner } from '$lib/components/ui/spinner'
	import IconPhotoX from '~icons/tabler/photo-x'
	import { convertToWebp } from '$lib/utils/convertToWebp'
	import { deleteAvatar, getUser, uploadAvatar, type UserId } from '$lib/utils/client'
	import { authClient } from '$lib/client'

	type ProfileFormData = {
		username: string
		email: string
	}

	let formData = $state<ProfileFormData>()

	let isUploading: boolean = $state(false)
	let isDeleting: boolean = $state(false)
	let avatarError: boolean = $state(false)

	const session = authClient.useSession()
	const userId = $derived($session.data?.user?.userId)

	$effect(() => {
		formData = undefined
		if (userId === undefined) return

		const controller = new AbortController()
		void loadProfile(userId, controller.signal)

		return () => controller.abort()
	})

	async function loadProfile(profileUserId: UserId, signal: AbortSignal) {
		try {
			const result = await getUser<true>({ path: { userId: profileUserId }, signal })
			if (signal.aborted) return

			formData = { username: result.data.username, email: result.data.email }
		} catch (error) {
			if (!signal.aborted) console.error('Failed to load profile:', error)
		}
	}

	let errors: Record<string, string> = $state({})

	let fileInput = $state<HTMLInputElement>()

	function handleClick() {
		fileInput?.click()
	}

	async function upload(
		event: Event & {
			currentTarget: EventTarget & HTMLInputElement
		}
	) {
		isUploading = true
		try {
			const files = event.currentTarget.files
			if (files == null) return
			if (files.length !== 1) return
			const file = files[0]
			if (file) {
				const blob = await convertToWebp(file)
				await uploadAvatar({ body: { file: blob } })
				avatarError = false
				//if (currentUser.user !== undefined) setCurrentUserAvatarUrl(currentUser.user.id, true)
			}
		} finally {
			isUploading = false
		}
	}

	async function handleDelete() {
		try {
			isDeleting = true
			await deleteAvatar()
		} finally {
			isDeleting = false
			avatarError = true
			//if (currentUser.user !== undefined) setCurrentUserAvatarUrl(undefined)
		}
	}
</script>

<div class="grid grid-cols-1 gap-y-4 md:grid-cols-[auto_1fr] md:gap-4">
	{#if formData != null}
		<Card.Root class="grid min-w-48">
			<Card.Header class="space-y-1">
				<Card.Title class="text-2xl">Avatar</Card.Title>
				<Card.Description>Edit your avatar</Card.Description>
			</Card.Header>
			<Card.Content>
				<div class="relative grid h-32 md:w-36 lg:w-64">
					{#if !avatarError}
						<img
							src={$session.data?.user?.avatarUrl}
							alt={$session.data?.user?.name}
							class="h-full max-h-32 w-full max-w-32 place-self-center rounded-lg border object-contain shadow-sm"
							onerror={() => {
								avatarError = true
							}}
						/>
					{:else}
						<div
							class="grid h-full max-h-32 w-full max-w-32 place-self-center rounded-lg border-2 border-dashed shadow-sm"
						>
							<IconPhotoX class="h-8 w-8 place-self-center text-muted" />
						</div>
					{/if}
				</div>
			</Card.Content>
			<Card.Footer class="grid w-44 grid-flow-col gap-x-4 place-self-center">
				<Button onclick={handleClick} disabled={isUploading || isDeleting}>
					{#if isUploading}
						<Spinner class="size-6" />
					{:else}
						<IconCamera class="size-6" />
					{/if}

					<input
						type="file"
						class="hidden"
						onchange={(e) => upload(e)}
						bind:this={fileInput}
					/></Button
				>
				<Button variant="destructive" onclick={handleDelete} disabled={isUploading || isDeleting}>
					{#if isDeleting}
						<Spinner class="size-6" />
					{:else}
						<IconTrash class="size-6" />
					{/if}</Button
				>
			</Card.Footer>
		</Card.Root>
		<Card.Root>
			<Card.Header class="space-y-1">
				<Card.Title class="text-2xl">Account</Card.Title>
				<Card.Description>Manage your account settings</Card.Description>
			</Card.Header>
			<Card.Content>
				<Field.FieldGroup>
					<Field.Field data-disabled="true" data-invalid={errors.username ? 'true' : undefined}>
						<Field.FieldLabel for="username">Username</Field.FieldLabel>
						<Input
							type="text"
							id="username"
							bind:value={formData.username}
							disabled
							aria-invalid={errors.username ? true : undefined}
						/>
						<Field.FieldError>{errors.username}</Field.FieldError>
					</Field.Field>
					<Field.Field data-disabled="true" data-invalid={errors.email ? 'true' : undefined}>
						<Field.FieldLabel for="email">Email</Field.FieldLabel>
						<Input
							type="email"
							id="email"
							bind:value={formData.email}
							disabled
							aria-invalid={errors.email ? true : undefined}
						/>
						<Field.FieldError>{errors.email}</Field.FieldError>
					</Field.Field>
				</Field.FieldGroup>
			</Card.Content>
			<Card.Footer>
				<!-- <Button class="w-full">Update account</Button> -->
			</Card.Footer>
		</Card.Root>
	{/if}
</div>
