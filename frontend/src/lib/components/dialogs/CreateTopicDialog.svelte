<script lang="ts">
  import { Button } from '$lib/components/ui/button'
  import * as Dialog from '$lib/components/ui/dialog'
  import { Input } from '$lib/components/ui/input'
  import { Label } from '$lib/components/ui/label'
  import { POST } from '$lib/utils/POST'
  import { IconMessagePlus, IconLoader2 } from '@tabler/icons-svelte'

  let { categoryId } = $props()
  let title: string = $state('Новый раздел')
  let openDialog: boolean = $state(false)
  let isLoading: boolean = $state(false)

  const createCategory = async () => {
    isLoading = true
    const response = await POST(`/topics`, { categoryId, title })
    isLoading = false
    if (response.ok) {
      const data = await response.json()
      openDialog = false
    } else {
      console.error('Ошибка при создании раздела:', response.statusText)
    }
  }
</script>

<Dialog.Root bind:open={openDialog}>
  <Dialog.Trigger>
    <Button class="h-8" size="sm" onclick={() => (openDialog = true)}>
      <IconMessagePlus class="h-3.5 w-3.5" />
      <span class="sr-only sm:not-sr-only sm:whitespace-nowrap"
        >Создать тему</span
      >
    </Button>
  </Dialog.Trigger>
  <Dialog.Content
    interactOutsideBehavior={isLoading ? 'ignore' : 'close'}
    class="sm:max-w-[425px] ${isLoading
      ? 'opacity-50 pointer-events-none'
      : ''}"
  >
    <Dialog.Header>
      <Dialog.Title>Создание темы</Dialog.Title>
    </Dialog.Header>
    <div class="grid gap-4 py-4">
      <div class="grid grid-cols-4 items-center gap-4">
        <Label for="name" class="text-right">Название темы</Label>
        <Input id="name" bind:value={title} class="col-span-3" />
      </div>
    </div>
    <Dialog.Footer>
      <Button type="button" onclick={createCategory} disabled={isLoading}>
        {#if isLoading}
          <IconLoader2 class="mr-2 h-4 w-4 animate-spin" />
          Отправка
        {:else}
          Создать
        {/if}
      </Button>
    </Dialog.Footer>
  </Dialog.Content>
</Dialog.Root>
