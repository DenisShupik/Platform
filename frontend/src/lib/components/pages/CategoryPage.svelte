<script lang="ts">
  import { get } from '$lib/get'
  import type { KeysetPage } from '$lib/types/KeysetPage'
  import type { Topic } from '$lib/types/Topic'
  import TopicView from '$lib/components/TopicView.svelte'

  let { categoryId }: { categoryId: Pick<Topic, 'categoryId'> } = $props()

  let promise: Promise<KeysetPage<Topic>> = $state(
    get<KeysetPage<Topic>>(`/categories/${categoryId}/topics`)
  )
</script>

{#await promise}
  <p>Загрузка тем...</p>
{:then topics}
  <div class="space-y-4 pt-8">
    {#each topics.items as topic}
      <TopicView {topic} />
    {/each}
  </div>
{:catch error}
  <p style="color: red;">{error}</p>
{/await}
