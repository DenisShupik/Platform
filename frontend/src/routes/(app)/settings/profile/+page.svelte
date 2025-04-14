<script lang="ts">
	import { Button } from '$lib/components/ui/button'
	import { Input } from '$lib/components/ui/input'
	import { Label } from '$lib/components/ui/label'
	import { z } from 'zod'
	import * as Card from '$lib/components/ui/card'
	import { IconCamera, IconTrash, IconLoader2, IconPhotoX } from '@tabler/icons-svelte'
	import { convertToWebp } from '$lib/utils/convertToWebp'
	import { authStore, currentUser, setCurrentUserAvatarUrl } from '$lib/client/auth-state.svelte'
	import { deleteAvatar, getUserById, uploadAvatar, type UserDto } from '$lib/utils/client'
	
	let formData:
		| {
				username: string
				email: string
		  }
		| undefined = $state()

	let isUploading: boolean = $state(false)
	let isDeleting: boolean = $state(false)
	let avatarError: boolean = $state(false)

	let user: UserDto | undefined = $state(undefined)

	$effect(() => {
		const userId = $currentUser?.id
		if (userId !== undefined && user === undefined) {
			getUserById<true>({ path: { userId } }).then((v) => {
				user = v.data
			})
		}
	})

	$effect(() => {
		if (formData == null && user != null) {
			formData = { username: user.username, email: user.email }
		}
	})

	const schema = z.object({
		username: z.string().min(3, 'Имя пользователя должно содержать не менее 3 символов'),
		email: z.string().email('Некорректный адрес электронной почты')
	})

	let errors: Record<string, string> = $state({})

	const handleSubmit = (event: Event) => {
		event.preventDefault()
		errors = {}

		try {
			schema.parse(formData)
		} catch (err) {
			if (err instanceof z.ZodError) {
				err.errors.forEach((error) => {
					errors[error.path[0]] = error.message
				})
			}
		}
	}

	let fileInput: HTMLInputElement | undefined = $state()

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
				await uploadAvatar({ body: { file: blob }, auth: $authStore.token })
				avatarError = false
				if ($currentUser !== undefined) setCurrentUserAvatarUrl($currentUser.id, true)
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
			if ($currentUser !== undefined) setCurrentUserAvatarUrl(undefined)
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
			<Card.Content class="grid gap-4">
				<div class="relative grid h-32 md:w-36 lg:w-64">
					{#if !avatarError}
						<img
							src={$currentUser?.avatarUrl}
							alt={$currentUser?.username}
							class="h-full max-h-[128px] w-full max-w-[128px] place-self-center rounded-lg border object-contain shadow-sm"
							onerror={() => {
								avatarError = true
							}}
						/>
					{:else}
						<div
							class="grid h-full max-h-[128px] w-full max-w-[128px] place-self-center rounded-lg border-2 border-dashed shadow-sm"
						>
							<IconPhotoX class="text-muted h-8 w-8 place-self-center" />
						</div>
					{/if}
				</div>
			</Card.Content>
			<Card.Footer class="grid w-44 grid-flow-col gap-x-4 place-self-center">
				<Button onclick={handleClick} disabled={isUploading || isDeleting}>
					{#if isUploading}
						<IconLoader2 class="h-6 w-6 animate-spin" />
					{:else}
						<IconCamera class="h-6 w-6" />
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
						<IconLoader2 class="h-6 w-6 animate-spin" />
					{:else}
						<IconTrash class="h-6 w-6" />
					{/if}</Button
				>
			</Card.Footer>
		</Card.Root>
		<Card.Root>
			<Card.Header class="space-y-1">
				<Card.Title class="text-2xl">Account</Card.Title>
				<Card.Description>Manage your account settings</Card.Description>
			</Card.Header>
			<Card.Content class="grid gap-4">
				<div class="flex w-full flex-col gap-1.5">
					<Label for="username" class={`font-bold ${errors.username ? 'text-red-600' : ''}`}
						>Username</Label
					>
					<Input type="text" id="username" bind:value={formData.username} disabled />
					{#if errors.username}
						<span class="text-sm text-red-600">{errors.username}</span>
					{/if}
				</div>
				<div class="flex w-full flex-col gap-1.5">
					<Label for="email" class={`font-bold ${errors.email ? 'text-red-600' : ''}`}>Email</Label>
					<Input type="email" id="email" bind:value={formData.email} disabled />
					{#if errors.email}
						<span class="text-sm text-red-600">{errors.email} </span>
					{/if}
				</div>
			</Card.Content>
			<Card.Footer>
				<!-- <Button class="w-full">Update account</Button> -->
			</Card.Footer>
		</Card.Root>
	{/if}
</div>
