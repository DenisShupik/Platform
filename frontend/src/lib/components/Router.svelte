<script>
  import { route, RouteKey } from '$lib/routeStore'
  import SectionPage from '$lib/components/pages/SectionPage.svelte'
  import CategoryPage from '$lib/components/pages/CategoryPage.svelte'
</script>

<main class="container">
  {#if $route?.key === RouteKey.Sections}
    <SectionPage />
  {:else if $route?.key === RouteKey.Category}
    <CategoryPage categoryId={$route?.params.categoryId} />
  {:else}
    <p>Страница не найдена</p>
  {/if}
</main>
