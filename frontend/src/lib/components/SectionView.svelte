<script lang="ts">
  import * as Collapsible from '$lib/components/ui/collapsible'
  import { buttonVariants } from '$lib/components/ui/button'
  import { Separator } from '$lib/components/ui/separator'
  import CategoryView from './CategoryView.svelte'
  import type { Section } from '$lib/types/Section'
  import CreateCategoryDialog from './dialogs/CreateCategoryDialog.svelte'
  import { IconChevronUp } from '@tabler/icons-svelte'

  let { section }: { section: Section } = $props()
  let isOpen = $state(true)
</script>

<Collapsible.Root
  class="bg-card text-card-foreground grid w-full rounded-b-lg border shadow-sm"
  bind:open={isOpen}
>
  <div class="bg-muted/40 flex items-center px-4">
    <h4 class="text-sm font-semibold">{section.title}</h4>
    <div class="ml-auto flex">
      <CreateCategoryDialog sectionId={section.sectionId} />
      <Collapsible.Trigger
        class={buttonVariants({
          variant: 'ghost',
          size: 'sm',
          class: 'w-9 p-0'
        })}
      >
        <IconChevronUp
          class={`transition-transform duration-200 ${isOpen ? 'rotate-180' : 'rotate-0'}`}
        />
        <span class="sr-only">Toggle</span>
      </Collapsible.Trigger>
    </div>
  </div>
  {#if section.categories != null && section.categories.length !== 0}
    <Collapsible.Content class="px-4 py-2">
      {#each section.categories ?? [] as category, index}
        <CategoryView {category} />
        {#if index < (section.categories?.length ?? 0) - 1}
          <Separator class="my-2" />
        {/if}
      {/each}
    </Collapsible.Content>
  {/if}
</Collapsible.Root>
