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
  import type { FetchPageContext } from '$lib/types/fetchPageContext'
  import { categoryPostsCountState } from '$lib/states/categoryPostsCountState.svelte'
  import {
    threadPostsCountLoader,
    threadPostsCountState
  } from '$lib/states/threadPostsCountState.svelte'
  import { threadLatestPostState } from '$lib/states/threadLatestPostState.svelte'
  import { categoryLatestPostState } from '$lib/states/categoryLatestPostState.svelte'

  let threadId: Thread['threadId'] = $derived(parseInt($page.params.threadId))
  let perPage = $state(5)
  let currentPage: number = $derived(getPageFromUrl($page.url))

  let postCount: number | undefined = $derived(
    threadPostsCountState.get(threadId)
  )

  let pageState: {
    pages: (Post[] | undefined)[]
  } = $state({ pages: [] })

  $effect(() => {
    if (postCount !== undefined) return
    threadPostsCountLoader
      .load(threadId)
      .then((v) => threadPostsCountState.set(threadId, v))
  })

  let fetchPageContext: FetchPageContext
  $effect(() => {
    const pageId = currentPage
    if (pageState.pages[pageId] === undefined) {
      if (fetchPageContext) {
        if (fetchPageContext.pageId === pageId) return
        fetchPageContext.abortController.abort()
      }
      const abortController = new AbortController()
      const signal = abortController.signal
      fetchPageContext = { abortController, pageId }
      getThreadPosts({
        path: { threadId },
        query: { cursor: (currentPage - 1) * perPage, limit: perPage },
        signal
      })
        .then((v) => {
          pageState.pages[pageId] = v.data?.items
        })
        .catch((error) => {
          if (error.name !== 'AbortError') throw error
        })
        .finally(() => {
          if (fetchPageContext?.abortController === abortController)
            fetchPageContext = undefined
        })
    }
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
    categoryLatestPostState.delete(thread.categoryId)
    categoryPostsCountState.delete(thread.categoryId)
    threadLatestPostState.delete(thread.threadId)
    threadPostsCountState.delete(thread.threadId)
    pageState = { pages: [] }
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

<Paginator {perPage} count={postCount} />

<section class="mt-4 grid gap-y-4">
  {#if pageState.pages[currentPage] != null}
    {#each pageState.pages[currentPage] ?? [] as post}
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
