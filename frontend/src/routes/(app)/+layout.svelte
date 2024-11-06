<script lang="ts">
  import '../../app.css'
  import { ModeWatcher } from 'mode-watcher'
  import { authStore } from '$lib/stores/authStore'
  import { SiteHeader } from '$lib/components'

  let { children } = $props()
</script>

<ModeWatcher />
{#if $authStore != null}
  <div class="bg-background relative flex min-h-screen flex-col">
    <SiteHeader />
    <main class="container pt-8">
      {@render children()}
    </main>
  </div>
{/if}
