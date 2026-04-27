<script lang="ts">
	import * as Breadcrumb from '$lib/components/ui/breadcrumb'
	import { Button, buttonVariants } from '$lib/components/ui/button'
	import { Spinner } from '$lib/components/ui/spinner'
	import {
		ButtonTitle,
		ForumBreadcrumb,
		Paginator,
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
	import IconMessageCheck from '~icons/tabler/message-check'
	import IconMessageQuestion from '~icons/tabler/message-question'
	import IconMessageX from '~icons/tabler/message-x'
	import IconPencil from '~icons/tabler/pencil'
	import { authClient } from '$lib/client'
	import { superForm } from 'sveltekit-superforms'
	import { postSchema } from './utils'
	import { valibotClient } from 'sveltekit-superforms/adapters'
	import * as Form from '$lib/components/ui/form'
	import * as InputGroup from '$lib/components/ui/input-group'
	import { PostContentSchema } from '$lib/utils/client/schemas.gen'
	import { untrack } from 'svelte'
	import { Role, roleAtLeast } from '$lib/roles'
	import CategoryBreadcrumb from '$lib/components/app/category-breadcrumb.svelte'

	let { data }: PageProps = $props()

	let startPostIndex = $derived((data.currentPage - 1) * data.perPage + 1)
	let isSubscribed = $state(untrack(() => data.isSubscribed))
	let threadState = $state(untrack(() => data.thread.state))

	const form = superForm(
		untrack(() => data.form),
		{
			validators: valibotClient(postSchema)
		}
	)

	const { form: formData, enhance } = form

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
				await requestThreadApproval<true>({
					path: { threadId: data.thread.threadId },
					signal: controller.signal
				})
				threadState = ThreadState.PENDING_APPROVAL
			} else if (action === ThreadAction.Approve) {
				await approveThread<true>({
					path: { threadId: data.thread.threadId },
					signal: controller.signal
				})
				threadState = ThreadState.APPROVED
			} else if (action === ThreadAction.Reject) {
				await rejectThread<true>({
					path: { threadId: data.thread.threadId },
					signal: controller.signal
				})
				threadState = ThreadState.DRAFT
			}
		} catch (error: any) {
			if (error?.name !== 'AbortError') {
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

<div class="px-4 sm:px-0">
	<Breadcrumb.Root>
		<Breadcrumb.List>
			<ForumBreadcrumb forum={data.forum} />
			<Breadcrumb.Separator />
			<CategoryBreadcrumb category={data.category} />
		</Breadcrumb.List>
	</Breadcrumb.Root>

	<h1 class="pb-2 text-xl font-bold sm:text-2xl">{data.thread.title}</h1>

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
						<Spinner />Cancel
					{:else}
						<IconMessageQuestion class="size-4" />
						<ButtonTitle class="sm:whitespace-nowrap">Request approval</ButtonTitle>
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
						<Spinner />Cancel
					{:else}
						<IconMessageCheck class="size-4" />
						<ButtonTitle>Approve</ButtonTitle>
					{/if}
				</Button>
				<Button
					class={buttonVariants({ class: 'h-8' })}
					variant="destructive"
					disabled={threadActionInProgress && currentAction !== ThreadAction.Reject}
					onclick={() => handleThreadAction(ThreadAction.Reject)}
				>
					{#if currentAction === ThreadAction.Reject}
						<Spinner />Cancel
					{:else}
						<IconMessageX class="size-4" />
						<ButtonTitle>Reject</ButtonTitle>
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
				index={startPostIndex + index}
				author={data.threadData.users.get(post.createdBy)}
			>
				{#if threadState !== ThreadState.PENDING_APPROVAL && post.createdBy == $session.data?.user?.userId}
					<Button onclick={() => editPost(post)} variant="ghost" class="size-8 cursor-pointer">
						<IconPencil />
					</Button>
				{/if}
			</PostView>
		{/each}
	</section>
{/if}

{#if threadState != ThreadState.PENDING_APPROVAL && $session.data}
	<form method="POST" use:enhance>
		{#if $formData.postId}
			<input type="hidden" name="postId" value={$formData.postId} />
			<input type="hidden" name="rowVersion" value={$formData.rowVersion} />
		{/if}

		<Form.Field {form} name="content">
			<Form.Control>
				{#snippet children({ props })}
					<InputGroup.Root class="mt-4 h-64 w-full border-0 bg-muted/40 sm:border sm:bg-muted/0">
						<InputGroup.Textarea
							{...props}
							id="post-editor"
							placeholder="Type your message here"
							bind:value={$formData.content}
						/>
						<InputGroup.Addon align="block-end">
							<InputGroup.Text class="text-xs text-muted-foreground">
								{charactersLeft} characters left
							</InputGroup.Text>
						</InputGroup.Addon>
					</InputGroup.Root>
				{/snippet}
			</Form.Control>
			<Form.FieldErrors />
		</Form.Field>
		<div class="flex gap-2 px-4 sm:px-0">
			{#if !$formData.postId}
				<Form.Button class="mt-4 ml-auto">Send</Form.Button>
			{:else}
				<Form.Button class="mt-4 ml-auto" variant="destructive" onclick={clearEdit}
					>Cancel</Form.Button
				>
				<Form.Button class="mt-4">Update</Form.Button>
			{/if}
		</div>
	</form>
{/if}
