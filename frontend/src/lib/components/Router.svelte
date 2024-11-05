<script>
  import { route, RouteKey } from '$lib/stores/routeStore'
  import SectionPage from '$lib/components/pages/SectionPage.svelte'
  import CategoryPage from '$lib/components/pages/CategoryPage.svelte'
  import TopicPage from '$lib/components/pages/TopicPage.svelte'
  import SettingsPage from '$lib/components/pages/SettingsPage.svelte'
</script>

<main class="container pt-8">
  {#if $route?.key === RouteKey.Sections}
    <SectionPage />
  {:else if $route?.key === RouteKey.Categories}
    <CategoryPage categoryId={parseInt($route?.params.categoryId)} />
  {:else if $route?.key === RouteKey.Topics}
    <TopicPage topicId={parseInt($route?.params.topicId)} />
  {:else if $route?.key === RouteKey.Users || $route?.key === RouteKey.UsersCurrentProfile || $route?.key === RouteKey.UsersCurrentAvatar}
    <SettingsPage />
  {:else}
    <p>Страница не найдена</p>
  {/if}
</main>
