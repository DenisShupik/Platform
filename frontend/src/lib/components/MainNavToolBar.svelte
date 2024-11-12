<script lang="ts">
  import { page } from '$app/stores'
  import CreateForumDialog from '$lib/components/dialogs/CreateForumDialog.svelte'
  import CreateThreadDialog from '$lib/components/dialogs/CreateThreadDialog.svelte'
  import CreateCategoryDialog from './dialogs/CreateCategoryDialog.svelte'
  import { buttonVariants } from './ui/button'

  const className = buttonVariants({ class: 'ml-2 h-8' })
</script>

<div>
  {#if $page.route.id === '/(app)'}
    <CreateForumDialog class={className} />
  {:else if $page.route.id === '/(app)/forums/[forumId]'}
    <CreateCategoryDialog
      forumId={parseInt($page.params.forumId)}
      class={className}
    />
  {:else if $page.route.id === '/(app)/categories/[categoryId]'}
    <CreateThreadDialog
      categoryId={parseInt($page.params.categoryId)}
      class={className}
    />
  {:else}{/if}
</div>
