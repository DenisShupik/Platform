<script lang="ts">
  import { get } from '$lib/get'
  import type { KeysetPage } from '$lib/types/KeysetPage'
  import SectionView from '$lib/components/SectionView.svelte'
  import type { Section } from '$lib/types/Section'
  import type { CategoryStats } from '$lib/types/CategoryStats'

  let stats: Map<number, CategoryStats> = $state(
    new Map<number, CategoryStats>()
  )

  const init = async () => {
    const sections = await get<KeysetPage<Section>>('/sections')
    stats = new Map(
      sections.items
        .flatMap((e) => e.categories?.map((e) => e.categoryId) ?? [])
        .map((e) => [e, { categoryId: e, topicCount: 0 }])
    )
    return sections
  }

  let promise: Promise<KeysetPage<Section>> = $state(init())
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
