<script lang="ts" module>
  import { getContext as svelteGetContext } from 'svelte'

  export interface Context {
    stats?: Map<number, CategoryStats>
  }

  export function getContext(): Context {
    return svelteGetContext('stats')
  }
</script>

<script lang="ts">
  import { GET } from '$lib/GET'
  import type { KeysetPage } from '$lib/types/KeysetPage'
  import SectionView from '$lib/components/SectionView.svelte'
  import type { Section } from '$lib/types/Section'
  import type { CategoryStats } from '$lib/types/CategoryStats'
  import { setContext } from 'svelte'

  let context: Context = $state({})
  setContext('stats', context)

  const init = async () => {
    const sections = await GET<KeysetPage<Section>>('/sections')
    const categoryIds = sections.items
      .flatMap((e) => e.categories?.map((e) => e.categoryId) ?? [])
      .join(',')
    Promise.resolve(
      GET<CategoryStats[]>(`/categories/${categoryIds}/stats`)
    ).then((e) => {
      context.stats = new Map(e.map((e) => [e.categoryId, e]))
    })
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
