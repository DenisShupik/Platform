<script lang="ts">
  import { page } from '$app/stores'
  import CreateSectionDialog from '$lib/components/dialogs/CreateSectionDialog.svelte'
  import CreateTopicDialog from '$lib/components/dialogs/CreateTopicDialog.svelte'
</script>

<div>
  {#if $page.route.id === '/(app)'}
    <CreateSectionDialog />
  {:else if $page.route.id === '/(app)/categories/[categoryId]'}
    <CreateTopicDialog categoryId={parseInt($page.params.categoryId)} />
  {:else}{/if}
</div>
