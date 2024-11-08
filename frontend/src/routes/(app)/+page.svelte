<script lang="ts">
  import SectionView from '$lib/components/ForumView.svelte'
  import { getForums } from '$lib/utils/client'

  let promise = $state(getForums<true>())
</script>

{#await promise}
  <p>Загрузка разделов...</p>
{:then sections}
  <div class="space-y-4">
    {#each sections.data.items ?? [] as section}
      <SectionView forum={section} />
    {/each}
  </div>
{:catch error}
  <p style="color: red;">{error}</p>
{/await}
