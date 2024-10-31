<script lang="ts">
  import * as Breadcrumb from '$lib/components/ui/breadcrumb'
  import BreadcrumbRouteLink from '$lib/components/ui/route-link/BreadcrumbRouteLink.svelte'
  import { GET } from '$lib/GET'
  import { categoryStore } from '$lib/stores/categoryStore'
  import { sectionStore } from '$lib/stores/sectionStore'
  import { topicStore } from '$lib/stores/topicStore'
  import type { Category } from '$lib/types/Category'
  import type { Section } from '$lib/types/Section'
  import type { Topic } from '$lib/types/Topic'

  let { topicId }: { topicId: Topic['topicId'] } = $props()

  let topic = $derived($topicStore.get(topicId))
  let category = $derived(
    topic === undefined ? undefined : $categoryStore.get(topic.categoryId)
  )
  let section = $derived(
    category === undefined ? undefined : $sectionStore.get(category.sectionId)
  )

  $effect(() => {
    if (topic === undefined) {
      GET<Topic>(`/topics/${topicId}`).then((v) =>
        topicStore.update((e) => {
          e.set(topicId, v)
          return e
        })
      )
    }
  })

  $effect(() => {
    if (topic !== undefined && category === undefined) {
      GET<Category>(`/categories/${topic.categoryId}`).then((v) =>
        categoryStore.update((e) => {
          e.set(topic.categoryId, v)
          return e
        })
      )
    }
  })

  $effect(() => {
    if (category !== undefined && section === undefined) {
      GET<Section>(`/sections/${category.sectionId}`).then((v) =>
        sectionStore.update((e) => {
          e.set(category.sectionId, v)
          return e
        })
      )
    }
  })

  // async function init() {
  //   const topic = await GET<Topic>(`/topics/${topicId}`)
  //   const category = categoryStore.get(topic.categoryId)
  //   const section = await sectionStore.get(category.sectionId)
  //   const [postCount, posts] = await Promise.all([
  //     GET<number>(`/topics/${topicId}/posts/count`),
  //     GET<KeysetPage<Post>>(`/topics/${topicId}/posts`)
  //   ])
  //   return { postCount, posts }
  // }
</script>

<Breadcrumb.Root>
  <Breadcrumb.List>
    <Breadcrumb.Item>
      <BreadcrumbRouteLink link="/" title="Разделы" />
    </Breadcrumb.Item>
    <Breadcrumb.Separator />
    <Breadcrumb.Item>
      {#if section}
        <BreadcrumbRouteLink
          link={`/sections/${section.sectionId}`}
          title={section.title}
        />
      {:else}
        <div>Получаю данные</div>
      {/if}
    </Breadcrumb.Item>
    <Breadcrumb.Separator />
    <Breadcrumb.Item>
      {#if category}
        <BreadcrumbRouteLink
          link={`/categories/${category.categoryId}`}
          title={category.title}
        />
      {:else}
        <div>Получаю данные</div>
      {/if}
    </Breadcrumb.Item>
  </Breadcrumb.List>
</Breadcrumb.Root>
