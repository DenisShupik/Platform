<script>
  import { route, RouteKey } from '$lib/stores/routeStore'
  import CreateSectionDialog from '$lib/components/dialogs/CreateSectionDialog.svelte'
  import CreateTopicDialog from '$lib/components/dialogs/CreateTopicDialog.svelte'
</script>

<main class="container">
  {#if $route?.key === RouteKey.Sections}
    <CreateSectionDialog />
  {:else if $route?.key === RouteKey.Categories}
    <CreateTopicDialog categoryId={parseInt($route?.params.categoryId)} />
  {:else}{/if}
</main>
