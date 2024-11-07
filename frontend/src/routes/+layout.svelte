<script lang="ts">
  import { authStore, exchange, initAuthCodeFlow } from '$lib/stores/authStore'
  import { goto } from '$app/navigation'
  import { page } from '$app/stores'

  let { children } = $props()

  async function init() {
    if ($authStore == null) {
      const urlParams = new URLSearchParams(window.location.search)
      const authCode = urlParams.get('code')
      if (authCode == null) {
        await initAuthCodeFlow($page.url)
      } else {
        await exchange()
        goto(sessionStorage.getItem('originalRoute') ?? '/')
      }
    }
  }
</script>

{#await init() then}
  {@render children()}
{/await}
