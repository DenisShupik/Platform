<script lang="ts">
  import { get } from '$lib/get'
  import type { KeysetPage } from '$lib/types/KeysetPage'
  import type { Topic } from '$lib/types/Topic'
  import TopicView from '$lib/components/TopicView.svelte'
  import * as Breadcrumb from '$lib/components/ui/breadcrumb/index.js'
  import type { Category } from '$lib/types/Category'
  import type { Section } from '$lib/types/Section'

  let { categoryId }: { categoryId: Pick<Category, 'categoryId'> } = $props()

  async function init() {
    const [category, topics] = await Promise.all([
      get<Category>(`/categories/${categoryId}`),
      get<KeysetPage<Topic>>(`/categories/${categoryId}/topics`)
    ])
    const section = await get<Section>(`/sections/${category.sectionId}`)
    return { section, category, topics }
  }

  let promise: Promise<{
    section: Section
    category: Category
    topics: KeysetPage<Topic>
  }> = $state(init())
</script>

{#await promise}
  <p>Загрузка тем...</p>
{:then { section, category, topics }}
  <Breadcrumb.Root>
    <Breadcrumb.List>
      <Breadcrumb.Item>
        <Breadcrumb.Link href="/">Разделы</Breadcrumb.Link>
      </Breadcrumb.Item>
      <Breadcrumb.Separator />
      <Breadcrumb.Item>
        <Breadcrumb.Link href={`/sections/${section.sectionId}`}
          >{section.title}</Breadcrumb.Link
        >
      </Breadcrumb.Item>
    </Breadcrumb.List>
  </Breadcrumb.Root>
  <div class="space-y-4 pt-8">
    {#each topics.items as topic}
      <TopicView {topic} />
    {/each}
  </div>
{:catch error}
  <p style="color: red;">{error}</p>
{/await}
