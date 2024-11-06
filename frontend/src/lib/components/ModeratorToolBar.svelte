<script lang="ts">
  import { page } from '$app/stores'
  import CreateSectionDialog from '$lib/components/dialogs/CreateSectionDialog.svelte'
  import CreateTopicDialog from '$lib/components/dialogs/CreateTopicDialog.svelte'
</script>

<main class="container">
  {#if $page.route.id === '/'}
    <CreateSectionDialog />
  {:else if $page.route.id === '/categories/[categoryId]'}
    <CreateTopicDialog categoryId={parseInt($page.params.categoryId)} />
  {:else}{/if}
</main>
