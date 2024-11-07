<script lang="ts">
  import * as Breadcrumb from '$lib/components/ui/breadcrumb'
  import BreadcrumbRouteLink from '$lib/components/ui/route-link/BreadcrumbRouteLink.svelte'
  import { GET } from '$lib/utils/GET'
  import { categoryStore } from '$lib/stores/categoryStore'
  import { sectionStore } from '$lib/stores/sectionStore'
  import { topicStore } from '$lib/stores/topicStore'
  import type { Category } from '$lib/types/Category'
  import type { KeysetPage } from '$lib/types/KeysetPage'
  import type { Post } from '$lib/types/Post'
  import type { Section } from '$lib/types/Section'
  import type { Topic } from '$lib/types/Topic'
  import { Skeleton } from '$lib/components/ui/skeleton'
  import { Textarea } from '$lib/components/ui/textarea'
  import { Button } from '$lib/components/ui/button'
  import { POST } from '$lib/utils/POST'
  import PostView from '$lib/components/PostView.svelte'
  import Paginator from '$lib/components/Paginator.svelte'
  import { page } from '$app/stores'
  import { postCountLoader } from '$lib/dataLoaders/postCountLoader'
  import { getPageFromUrl } from '$lib/utils/tryParseInt'

  let fetchAbortController: AbortController | null = null

  let topicId: Topic['topicId'] = $derived(parseInt($page.params.topicId))
  let perPage = $state(5)
  let currentPage: number = $state(getPageFromUrl($page.url))

  $effect(() => {
    if (topic !== undefined) {
      if (fetchAbortController) {
        fetchAbortController.abort()
      }
      const abortController = new AbortController()
      const signal = abortController.signal
      fetchAbortController = abortController
      GET<KeysetPage<Post>>(
        `/topics/${topicId}/posts?cursor=${(currentPage - 1) * perPage}&limit=${perPage}`,
        { signal }
      )
        .then((v) => (posts = v))
        .catch((error) => {
          if (error.name !== 'AbortError') throw error
        })
        .finally(() => {
          if (fetchAbortController === abortController)
            fetchAbortController = null
        })
    }
  })

  let postCount: number | undefined = $state()
  let posts: KeysetPage<Post> | undefined = $state()
  let content: string | undefined = $state()

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

  $effect(() => {
    if (topic !== undefined && postCount === undefined) {
      postCountLoader.load(topic.topicId).then((v) => (postCount = v))
    }
  })

  async function createPost() {
    if (topic?.topicId == null) return
    await POST(`/topics/${topic.topicId}/posts`, { content })
  }
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

<Paginator bind:page={currentPage} {perPage} count={postCount} />

<section class="mt-4 grid gap-y-4">
  {#if posts != null}
    {#each posts.items as post}
      <PostView {post} />
    {/each}
  {:else}
    <Skeleton class="h-32 w-full" />
    <Skeleton class="h-32 w-full" />
    <Skeleton class="h-32 w-full" />
    <Skeleton class="h-32 w-full" />
    <Skeleton class="h-32 w-full" />
    <Skeleton class="h-32 w-full" />
  {/if}
</section>

<Textarea
  class="mt-4 h-64 w-full "
  placeholder="Type your message here."
  bind:value={content}
/>
<div class="flex">
  <Button
    class="ml-auto mt-4"
    disabled={typeof content !== 'string' || content.trim().length < 1}
    onclick={createPost}>Отправить</Button
  >
</div>
