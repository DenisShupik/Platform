<script lang="ts">
  import * as Breadcrumb from '$lib/components/ui/breadcrumb'
  import BreadcrumbRouteLink from '$lib/components/ui/route-link/BreadcrumbRouteLink.svelte'
  import { categoryStore } from '$lib/stores/categoryStore'
  import { forumStore } from '$lib/stores/forumStore'
  import { threadStore } from '$lib/stores/threadStore'
  import { Skeleton } from '$lib/components/ui/skeleton'
  import { Textarea } from '$lib/components/ui/textarea'
  import { Button } from '$lib/components/ui/button'
  import PostView from '$lib/components/PostView.svelte'
  import Paginator from '$lib/components/Paginator.svelte'
  import { page } from '$app/stores'
  import { postCountLoader } from '$lib/dataLoaders/postCountLoader'
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

  let fetchAbortController: AbortController | null = null

  let threadId: Thread['threadId'] = $derived(parseInt($page.params.threadId))
  let perPage = $state(5)
  let currentPage: number = $state(getPageFromUrl($page.url))

  let posts: Post[] | undefined = $state()

  $effect(() => {
    if (thread !== undefined) {
      if (fetchAbortController) {
        fetchAbortController.abort()
      }
      const abortController = new AbortController()
      const signal = abortController.signal
      fetchAbortController = abortController

      getThreadPosts({
        path: { threadId },
        query: { cursor: (currentPage - 1) * perPage, limit: perPage },
        signal
      })
        .then((v) => (posts = v.data?.items))
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

  let content: string | undefined = $state()

  let thread = $derived($threadStore.get(threadId))
  let category = $derived(
    thread === undefined ? undefined : $categoryStore.get(thread.categoryId)
  )
  let forum = $derived(
    category === undefined ? undefined : $forumStore.get(category.forumId)
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

  $effect(() => {
    if (thread != null && postCount === undefined) {
      postCountLoader.load(thread.threadId).then((v) => (postCount = v))
    }
  })

  async function onCreatePost() {
    if (thread?.threadId == null) return
    await createPost({ path: { threadId: thread.threadId }, body: { content } })
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

<Paginator bind:page={currentPage} {perPage} count={postCount} />

<section class="mt-4 grid gap-y-4">
  {#if posts != null}
    {#each posts as post}
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
