<script lang="ts">
  import { authStore, exchange, initAuthCodeFlow } from '$lib/stores/authStore'
  import { goto } from '$app/navigation'

  let { children } = $props()

  async function init() {
    if ($authStore == null) {
      const urlParams = new URLSearchParams(window.location.search)
      const authCode = urlParams.get('code')
      if (authCode == null) {
        await initAuthCodeFlow()
      } else {
        await exchange()
        goto('/')
      }
    }
  }
</script>

{#await init() then}
  {@render children()}
{/await}
