<script lang="ts">
  import { page } from '$app/stores'
  import CreateForumDialog from '$lib/components/dialogs/CreateForumDialog.svelte'
  import CreateThreadDialog from '$lib/components/dialogs/CreateThreadDialog.svelte'
</script>

<div>
  {#if $page.route.id === '/(app)'}
    <CreateForumDialog />
  {:else if $page.route.id === '/(app)/categories/[categoryId]'}
    <CreateThreadDialog categoryId={parseInt($page.params.categoryId)} />
  {:else}{/if}
</div>
