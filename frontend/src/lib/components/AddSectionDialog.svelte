<script lang="ts">
  import { Button } from '$lib/components/ui/button'
  import * as Dialog from '$lib/components/ui/dialog'
  import { Input } from '$lib/components/ui/input'
  import { Label } from '$lib/components/ui/label'
  import { send } from '$lib/send'
  import { MessageSquarePlus } from 'lucide-svelte'

  let title: string = 'Новый раздел'

  const createSection = async () => {
    const response = await send('https://localhost:8000/api/sections', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ title })
    })

    if (response.ok) {
      const data = await response.json()
      console.log('Раздел создан:', data)
    } else {
      console.error('Ошибка при создании раздела:', response.statusText)
    }
  }
</script>

<Dialog.Root>
  <Dialog.Trigger>
    <Button>
      <MessageSquarePlus class="mr-2 h-4 w-4" />
      Добавить раздел
    </Button>
  </Dialog.Trigger>
  <Dialog.Content class="sm:max-w-[425px]">
    <Dialog.Header>
      <Dialog.Title>Создание раздела</Dialog.Title>
    </Dialog.Header>
    <div class="grid gap-4 py-4">
      <div class="grid grid-cols-4 items-center gap-4">
        <Label for="name" class="text-right">Название раздела</Label>
        <Input id="name" bind:value={title} class="col-span-3" />
      </div>
    </div>
    <Dialog.Footer>
      <Button type="button" on:click={createSection}>Создать</Button>
    </Dialog.Footer>
  </Dialog.Content>
</Dialog.Root>
