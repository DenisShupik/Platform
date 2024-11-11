<script lang="ts">
  import Forum from '$lib/components/ForumView.svelte'
  import { getForums } from '$lib/utils/client'

  let promise = $state(getForums<true>())
</script>

{#await promise}
  <p>Загрузка разделов...</p>
{:then forums}
  <div class="space-y-4">
    {#each forums.data.items ?? [] as forum}
      <Forum {forum} />
    {/each}
  </div>
{:catch error}
  <p style="color: red;">{error}</p>
{/await}
