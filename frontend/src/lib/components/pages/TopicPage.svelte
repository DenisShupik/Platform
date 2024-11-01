<script lang="ts">
  import * as Breadcrumb from '$lib/components/ui/breadcrumb'
  import BreadcrumbRouteLink from '$lib/components/ui/route-link/BreadcrumbRouteLink.svelte'
  import { GET } from '$lib/GET'
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
  import * as Pagination from '$lib/components/ui/pagination'
  import ChevronRight from '@tabler/icons-svelte/icons/chevron-right'
  import ChevronLeft from '@tabler/icons-svelte/icons/chevron-left'
  import { MediaQuery } from 'runed'
  import { Button } from '$lib/components/ui/button'
  import { POST } from '$lib/post'
  import PostView from '$lib/components/PostView.svelte'

  let { topicId }: { topicId: Topic['topicId'] } = $props()

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
      GET<number>(`/topics/${topic.topicId}/posts/count`).then(
        (v) => (postCount = v)
      )
    }
  })

  $effect(() => {
    if (topic !== undefined && posts === undefined) {
      GET<KeysetPage<Post>>(`/topics/${topic.topicId}/posts`).then(
        (v) => (posts = v)
      )
    }
  })

  const isDesktop = new MediaQuery('(min-width: 768px)')

  const perPage = $derived(isDesktop.matches ? 3 : 8)
  const siblingCount = $derived(isDesktop.matches ? 1 : 0)

  async function createPost() {
    if (topic?.topicId == null) return
    await POST('/posts', { topicId: topic.topicId, content })
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

{#if postCount !== undefined}
  <Pagination.Root count={postCount} {perPage} {siblingCount}>
    {#snippet children({ pages, currentPage })}
      <Pagination.Content>
        <Pagination.Item>
          <Pagination.PrevButton>
            <ChevronLeft class="size-4" />
            <span class="hidden sm:block">Previous</span>
          </Pagination.PrevButton>
        </Pagination.Item>
        {#each pages as page (page.key)}
          {#if page.type === 'ellipsis'}
            <Pagination.Item>
              <Pagination.Ellipsis />
            </Pagination.Item>
          {:else}
            <Pagination.Item>
              <Pagination.Link {page} isActive={currentPage === page.value}>
                {page.value}
              </Pagination.Link>
            </Pagination.Item>
          {/if}
        {/each}
        <Pagination.Item>
          <Pagination.NextButton>
            <span class="hidden sm:block">Next</span>
            <ChevronRight class="size-4" />
          </Pagination.NextButton>
        </Pagination.Item>
      </Pagination.Content>
    {/snippet}
  </Pagination.Root>
{/if}

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
  class="mt-4 w-full h-64"
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
