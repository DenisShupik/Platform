<script lang="ts">
  import * as Breadcrumb from '$lib/components/ui/breadcrumb'
  import BreadcrumbRouteLink from '$lib/components/ui/route-link/BreadcrumbRouteLink.svelte'
  import { categoryStore } from '$lib/states/categoryStore'
  import { forumStore } from '$lib/states/forumStore'
  import { threadStore } from '$lib/states/threadStore'
  import { Skeleton } from '$lib/components/ui/skeleton'
  import { Textarea } from '$lib/components/ui/textarea'
  import { Button } from '$lib/components/ui/button'
  import PostView from '$lib/components/PostView.svelte'
  import Paginator from '$lib/components/Paginator.svelte'
  import { page } from '$app/stores'
  import { getPageFromUrl } from '$lib/utils/tryParseInt'
  import {
    createPost,
    getCategory,
    getForum,
    getThread,
    getThreadPosts,
    type Post,
    type Thread
  } from '$lib/utils/client'
  import { createThreadPostsCountMap } from '$lib/states/threadPostsCountState.svelte'
  import { setContext } from 'svelte'

  let threadId: Thread['threadId'] = $derived(parseInt($page.params.threadId))
  let currentPage: number = $derived(getPageFromUrl($page.url))
  let perPage = $state(5)

  const init = (state: { postCount?: number; posts?: Post[] }) => {
    state.t = createThreadPostsCountMap();
    state.postCount = state.t.get(threadId)
  }

  let pageState = $state<{ postCount?: number; posts?: Post[] }>({})

  init(pageState)

  setContext('pageState', pageState)

  $effect(() => {
    if (pageState.posts !== undefined) return
    getThreadPosts({
      path: { threadId },
      query: { cursor: (currentPage - 1) * perPage, limit: perPage }
    })
      .then((v) => {
        pageState.posts = v.data?.items
      })
      .catch((error) => {
        if (error.name !== 'AbortError') throw error
      })
  })

  let content: string | undefined = $state()

  let thread = $derived($threadStore.get(threadId))
  let category = $derived(
    thread === undefined
      ? undefined
      : thread == null
        ? null
        : $categoryStore.get(thread.categoryId)
  )
  let forum = $derived(
    category === undefined
      ? undefined
      : category === null
        ? null
        : $forumStore.get(category.forumId)
  )

  $effect(() => {
    if (thread === undefined) {
      getThread<true>({ path: { threadId } }).then((v) =>
        threadStore.update((e) => {
          e.set(threadId, v.data)
          return e
        })
      )
    }
  })

  $effect(() => {
    if (thread != null && category === undefined) {
      const categoryId = thread.categoryId
      getCategory<true>({ path: { categoryId } }).then((v) =>
        categoryStore.update((e) => {
          e.set(categoryId, v.data)
          return e
        })
      )
    }
  })

  $effect(() => {
    if (category != null && forum === undefined) {
      const forumId = category.forumId
      getForum<true>({ path: { forumId } }).then((v) =>
        forumStore.update((e) => {
          e.set(forumId, v.data)
          return e
        })
      )
    }
  })

  async function onCreatePost() {
    if (thread?.threadId == null) return
    await createPost({ path: { threadId: thread.threadId }, body: { content } })
    pageState = init()
  }
</script>

<Breadcrumb.Root>
  <Breadcrumb.List>
    <Breadcrumb.Item>
      <BreadcrumbRouteLink link="/" title="Forums" />
    </Breadcrumb.Item>
    <Breadcrumb.Separator />
    <Breadcrumb.Item>
      {#if forum}
        <BreadcrumbRouteLink
          link={`/forums/${forum.forumId}`}
          title={forum.title}
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

<Paginator {perPage} count={pageState.postCount} />

<section class="mt-4 grid gap-y-4">
  {#if pageState.posts != null}
    {#each pageState.posts ?? [] as post}
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
    onclick={onCreatePost}>Отправить</Button
  >
</div>
