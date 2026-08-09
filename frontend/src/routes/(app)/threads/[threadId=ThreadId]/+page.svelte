<script lang="ts">
	import { withApiLocale } from '$lib/client/api-options'
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import { Button, buttonVariants } from '$lib/components/ui/button'
	import { Spinner } from '$lib/components/ui/spinner'
	import {
		ButtonTitle,
		ForumBreadcrumb,
		Paginator,
		PostBookmarkButton,
		PostMarkdownEditor,
		PostView,
		ThreadSubscriptionButton
	} from '$lib/components/app'
	import type { PageProps } from './$types'
	import {
		approveThread,
		rejectThread,
		requestThreadApproval,
		ThreadState,
		type PostDto
	} from '$lib/utils/client'
	import { createIndex } from '$lib/utils/value-object'
	import IconMessageCheck from '~icons/tabler/message-check'
	import IconMessageQuestion from '~icons/tabler/message-question'
	import IconMessageX from '~icons/tabler/message-x'
	import IconPencil from '~icons/tabler/pencil'
	import MessageSquareReplyIcon from '@lucide/svelte/icons/message-square-reply'
	import { authClient } from '$lib/client'
	import { superForm } from 'sveltekit-superforms'
	import { createPostSchema } from './utils'
	import { valibotClient } from 'sveltekit-superforms/adapters'
	import * as Form from '$lib/components/ui/form'
	import { PostContentSchema } from '$lib/utils/client/schemas.gen'
	import { untrack } from 'svelte'
	import { fromAction } from 'svelte/attachments'
	import { Role, roleAtLeast } from '$lib/roles'
	import CategoryBreadcrumb from '$lib/components/app/category-breadcrumb.svelte'
	import { PUBLIC_APP_NAME } from '$env/static/public'
	import * as m from '$lib/paraglide/messages'
	import { getLocale } from '$lib/paraglide/runtime'
	import { formatNumber } from '$lib/utils/format'

	let { data }: PageProps = $props()

	let startPostIndex = $derived((data.currentPage - 1) * data.perPage + 1)
	let isSubscribed = $state(untrack(() => data.isSubscribed))
	let threadState = $state(untrack(() => data.thread.state))

	const postSchema = createPostSchema()
	const form = superForm(
		untrack(() => data.form),
		{
			validators: valibotClient(postSchema, { config: { lang: getLocale() } })
		}
	)

	const { form: formData, enhance } = form
	const enhanceAttachment = fromAction(enhance)

	const ThreadAction = {
		RequestApproval: 'request-approval',
		Approve: 'approve',
		Reject: 'reject'
	} as const

	type ThreadAction = (typeof ThreadAction)[keyof typeof ThreadAction]

	let charactersLeft = $derived(PostContentSchema.maxLength - $formData.content.length)

	const session = authClient.useSession()

	const canRequestApproval = $derived(
		threadState == ThreadState.DRAFT &&
			data.postCount > 0 &&
			roleAtLeast($session.data?.user.role, Role.User) &&
			data.thread.createdBy === $session.data?.user.userId
	)

	const canApprove = $derived(
		threadState == ThreadState.PENDING_APPROVAL &&
			roleAtLeast($session.data?.user.role, Role.Moderator)
	)

	function editPost(post: PostDto) {
		$formData.postId = post.postId
		$formData.content = post.content
		$formData.rowVersion = post.rowVersion
		setTimeout(() => {
			const editor = document.getElementById('post-editor')
			editor?.focus({ preventScroll: true })
			editor?.scrollIntoView({ behavior: 'smooth' })
		}, 0)
	}

	function clearEdit() {
		$formData.postId = undefined
		$formData.content = ''
		$formData.rowVersion = undefined
	}

	let currentAbortController: AbortController | null = null
	let currentAction = $state<ThreadAction | null>(null)

	let threadActionInProgress = $derived(currentAction !== null)

	async function handleThreadAction(action: ThreadAction) {
		if (currentAbortController) {
			currentAbortController.abort()
			currentAbortController = null
			currentAction = null
			return
		}

		const controller = new AbortController()
		currentAbortController = controller
		currentAction = action

		try {
			if (action === ThreadAction.RequestApproval) {
				await requestThreadApproval<true>(
					withApiLocale({
						path: { threadId: data.thread.threadId },
						signal: controller.signal,
						throwOnError: true
					})
				)
				threadState = ThreadState.PENDING_APPROVAL
			} else if (action === ThreadAction.Approve) {
				await approveThread<true>(
					withApiLocale({
						path: { threadId: data.thread.threadId },
						signal: controller.signal,
						throwOnError: true
					})
				)
				threadState = ThreadState.APPROVED
			} else if (action === ThreadAction.Reject) {
				await rejectThread<true>(
					withApiLocale({
						path: { threadId: data.thread.threadId },
						signal: controller.signal,
						throwOnError: true
					})
				)
				threadState = ThreadState.DRAFT
			}
		} catch (error: unknown) {
			if (!(error instanceof Error && error.name === 'AbortError')) {
				throw error
			}
		} finally {
			currentAction = null
			if (currentAbortController === controller) {
				currentAbortController = null
			}
		}
	}
</script>

<svelte:head>
	<title>{data.thread.title} — {data.category.title} — {PUBLIC_APP_NAME}</title>
</svelte:head>

<div>
	<Breadcrumb.Root aria-label={m.breadcrumb_label()}>
		<Breadcrumb.List>
			<ForumBreadcrumb forum={data.forum} />
			<Breadcrumb.Separator />
			<CategoryBreadcrumb category={data.category} />
			<Breadcrumb.Separator />
			<Breadcrumb.Item>
				<Breadcrumb.Page>{data.thread.title}</Breadcrumb.Page>
			</Breadcrumb.Item>
		</Breadcrumb.List>
	</Breadcrumb.Root>

	<h1 class="mt-3 pb-2 text-xl font-bold sm:text-2xl">{data.thread.title}</h1>

	<div class="grid grid-cols-3 items-center">
		<div></div>
		<Paginator currentPage={data.currentPage} perPage={data.perPage} totalCount={data.postCount} />
		<div class="grid grid-flow-col justify-end gap-x-2">
			{#if canRequestApproval}
				<Button
					class={buttonVariants({ class: 'h-8' })}
					disabled={threadActionInProgress && currentAction !== ThreadAction.RequestApproval}
					onclick={() => handleThreadAction(ThreadAction.RequestApproval)}
				>
					{#if currentAction === ThreadAction.RequestApproval}
						<Spinner />{m.common_cancel()}
					{:else}
						<IconMessageQuestion class="size-4" />
						<ButtonTitle class="sm:whitespace-nowrap">{m.thread_request_approval()}</ButtonTitle>
					{/if}
				</Button>
			{/if}
			{#if canApprove}
				<Button
					class={buttonVariants({ class: 'h-8' })}
					disabled={threadActionInProgress && currentAction !== ThreadAction.Approve}
					onclick={() => handleThreadAction(ThreadAction.Approve)}
				>
					{#if currentAction === ThreadAction.Approve}
						<Spinner />{m.common_cancel()}
					{:else}
						<IconMessageCheck class="size-4" />
						<ButtonTitle>{m.thread_approve()}</ButtonTitle>
					{/if}
				</Button>
				<Button
					class={buttonVariants({ class: 'h-8' })}
					variant="destructive"
					disabled={threadActionInProgress && currentAction !== ThreadAction.Reject}
					onclick={() => handleThreadAction(ThreadAction.Reject)}
				>
					{#if currentAction === ThreadAction.Reject}
						<Spinner />{m.common_cancel()}
					{:else}
						<IconMessageX class="size-4" />
						<ButtonTitle>{m.thread_reject()}</ButtonTitle>
					{/if}
				</Button>
			{/if}
			<ThreadSubscriptionButton threadId={data.thread.threadId} bind:isSubscribed />
		</div>
	</div>
</div>

{#if data.threadData}
	<section class="mt-4 grid gap-y-4">
		{#each data.threadData.threadPosts ?? [] as post, index (post.postId)}
			<PostView
				{post}
				index={createIndex(startPostIndex + index)}
				author={data.threadData.users.get(post.createdBy)}
			>
				<PostBookmarkButton
					postId={post.postId}
					initialIsBookmarked={data.threadData.bookmarkedPostIds.includes(post.postId)}
				/>
				{#if threadState !== ThreadState.PENDING_APPROVAL && post.createdBy == $session.data?.user?.userId}
					<Button
						onclick={() => editPost(post)}
						variant="ghost"
						class="size-8 cursor-pointer"
						aria-label={m.post_edit()}
					>
						<IconPencil />
					</Button>
				{/if}
			</PostView>
		{/each}
	</section>
{/if}

{#if threadState != ThreadState.PENDING_APPROVAL && $session.data}
	<form class="mt-4 flex flex-col gap-3" method="POST" {@attach enhanceAttachment}>
		{#if $formData.postId}
			<input type="hidden" name="postId" value={$formData.postId} />
			<input type="hidden" name="rowVersion" value={$formData.rowVersion} />
		{/if}

		<Form.Field {form} name="content">
			<Form.Control>
				{#snippet children({ props })}
					<PostMarkdownEditor
						textarea={{
							...props,
							id: 'post-editor',
							maxLength: PostContentSchema.maxLength,
							required: true,
							spellcheck: true
						}}
						bind:value={$formData.content}
					>
						{#snippet footer()}
							<div class="min-w-0 flex-1">
								<Form.Description class="tabular-nums"
									>{m.post_characters_remaining({
										count: formatNumber(charactersLeft)
									})}</Form.Description
								>
								<Form.FieldErrors />
							</div>
							<div class="flex shrink-0 justify-end gap-2">
								{#if !$formData.postId}
									<Form.Button>
										<MessageSquareReplyIcon data-icon="inline-start" />
										{m.post_reply()}
									</Form.Button>
								{:else}
									<Button type="button" variant="outline" onclick={clearEdit}
										>{m.post_cancel_edit()}</Button
									>
									<Form.Button>{m.post_update()}</Form.Button>
								{/if}
							</div>
						{/snippet}
					</PostMarkdownEditor>
				{/snippet}
			</Form.Control>
		</Form.Field>
	</form>
{/if}
