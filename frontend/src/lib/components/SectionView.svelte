<script lang="ts">
  import * as Collapsible from '$lib/components/ui/collapsible'
  import { ChevronUp } from 'lucide-svelte'
  import { Button, buttonVariants } from '$lib/components/ui/button'
  import CategoryView from './CategoryView.svelte'
  import type { Section } from '$lib/types/Section'
  import CreateCategoryDialog from './CreateCategoryDialog.svelte'

  let { section }: { section: Section } = $props()
  let isOpen = $state(true)
</script>

<Collapsible.Root
  class="w-full bg-card text-card-foreground rounded-lg border shadow-sm space-y-2"
  bind:open={isOpen}
>
  <div class="bg-red-800 flex items-center px-4">
    <h4 class="text-sm font-semibold">{section.title}</h4>
    <div class="ml-auto flex space-x-4">
      <CreateCategoryDialog sectionId={section.sectionId} />
      <Collapsible.Trigger
        class={buttonVariants({
          variant: 'ghost',
          size: 'sm',
          class: 'w-9 p-0'
        })}
      >
        <ChevronUp class="size-4" />
        <span class="sr-only">Toggle</span>
      </Collapsible.Trigger>
    </div>
  </div>
  <Collapsible.Content class="space-y-2 p-4 pt-0">
    {#each section.categories ?? [] as category}
      <CategoryView {category} />
    {/each}
  </Collapsible.Content>
</Collapsible.Root>
