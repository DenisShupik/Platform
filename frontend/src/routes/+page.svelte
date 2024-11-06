<script lang="ts">
  import type { KeysetPage } from '$lib/types/KeysetPage'
  import SectionView from '$lib/components/SectionView.svelte'
  import type { Section } from '$lib/types/Section'
  import { GET } from '$lib/utils/GET'

  let promise: Promise<KeysetPage<Section>> = $state(
    GET<KeysetPage<Section>>('/sections')
  )
</script>

{#await promise}
  <p>Загрузка разделов...</p>
{:then sections}
  <div class="space-y-4">
    {#each sections.items as section}
      <SectionView {section} />
    {/each}
  </div>
{:catch error}
  <p style="color: red;">{error}</p>
{/await}
