<script lang="ts">
  import { GET } from '$lib/GET'
  import { currentUserStore } from '$lib/stores/currentUserStore'
  import { userStore } from '$lib/stores/userStore'
  import type { User } from '$lib/types/User'
  import { Button } from '../ui/button'
  import { Input } from '../ui/input'
  import { Label } from '../ui/label'
  import { z } from 'zod'
  import * as Card from '$lib/components/ui/card'
  import { IconCamera, IconTrash } from '@tabler/icons-svelte'

  let formData:
    | {
        username: string
        email: string
      }
    | undefined = $state()

  let user: User | undefined = $derived(
    $currentUserStore === undefined
      ? undefined
      : $userStore.get($currentUserStore?.userId)
  )

  $effect(() => {
    if ($currentUserStore !== undefined && user === undefined) {
      const id = $currentUserStore.userId
      GET<User>(`/users/${id}`).then((v) =>
        userStore.update((e) => {
          e.set(id, v)
          return e
        })
      )
    }
  })

  $effect(() => {
    if (formData == null && user != null) {
      formData = { username: user.username, email: user.email }
    }
  })

  const schema = z.object({
    username: z
      .string()
      .min(3, 'Имя пользователя должно содержать не менее 3 символов'),
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
</script>

<div class="grid md:grid-cols-[auto,1fr] grid-cols-1 md:gap-4 gap-y-4">
  {#if formData != null}
    <Card.Root class="min-w-48">
      <Card.Header class="space-y-1">
        <Card.Title class="text-2xl">Avatar</Card.Title>
        <Card.Description>Edit your avatar</Card.Description>
      </Card.Header>
      <Card.Content class="grid gap-4">
        <div class="grid relative md:w-36 lg:w-64">
          <img
            src={`https://localhost:9000/avatars/${$currentUserStore?.userId}.jpg`}
            alt={$currentUserStore?.username}
            class="max-w-[128px] max-h-[128px] w-full h-full object-contain shadow-sm border rounded-lg place-self-center"
          />
        </div>
      </Card.Content>
      <Card.Footer class="grid gap-x-4 grid-flow-col w-44 place-self-center">
        <Button><IconCamera class="w-6 h-6" /></Button>
        <Button><IconTrash class="w-6 h-6" /></Button>
      </Card.Footer>
    </Card.Root>
    <Card.Root>
      <Card.Header class="space-y-1">
        <Card.Title class="text-2xl">Account</Card.Title>
        <Card.Description>Manage your account settings</Card.Description>
      </Card.Header>
      <Card.Content class="grid gap-4">
        <div class="w-full flex flex-col gap-1.5">
          <Label
            for="username"
            class={`font-bold ${errors.username ? 'text-red-600' : ''}`}
            >Username</Label
          >
          <Input type="text" id="username" bind:value={formData.username} />
          {#if errors.username}
            <span class="text-red-600 text-sm">{errors.username}</span>
          {/if}
        </div>
        <div class="w-full flex flex-col gap-1.5">
          <Label
            for="email"
            class={`font-bold ${errors.email ? 'text-red-600' : ''}`}
            >Email</Label
          >
          <Input type="email" id="email" bind:value={formData.email} />
          {#if errors.email}
            <span class="text-red-600 text-sm">{errors.email}</span>
          {/if}
        </div>
      </Card.Content>
      <Card.Footer>
        <Button class="w-full">Update account</Button>
      </Card.Footer>
    </Card.Root>
  {/if}
</div>
