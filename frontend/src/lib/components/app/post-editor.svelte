<script lang="ts">
	import { Textarea } from '$lib/components/ui/textarea'
	import * as Form from '$lib/components/ui/form'
	import type { PostDto, PostId, ThreadId } from '$lib/utils/client'
	import { defaults, superForm } from 'sveltekit-superforms'
	import { valibot } from 'sveltekit-superforms/adapters'
	import { goto } from '$app/navigation'
	import { resolve } from '$app/paths'
	import { createPost, getPostIndex, updatePost } from '$lib/utils/client'
	import { currentUser } from '$lib/client/current-user-state.svelte'
	import { vCreatePostRequestBody, vUpdatePostRequestBody } from '$lib/utils/client/valibot.gen'

	let {
		threadId,
		perPage,
		editedPost = $bindable()
	}: {
		threadId: ThreadId
		perPage: bigint
		editedPost?: PostDto
	} = $props()

	async function navigateToPost(postId: PostId) {
		const postIndex = (await getPostIndex<true>({ path: { postId } })).data
		const newPageIndex = BigInt(postIndex) / perPage + 1n
		await goto(
			`${resolve('/(app)/threads/[threadId=ThreadId]', { threadId })}?page=${newPageIndex}#post-${postId}`,
			{ invalidateAll: true }
		)
	}

	function clearEdit() {
		editedPost = undefined
	}

	const createPostForm = superForm(defaults(valibot(vCreatePostRequestBody)), {
		SPA: true,
		validators: valibot(vCreatePostRequestBody),
		async onUpdate({ form }) {
			if (form.valid) {
				const postId = (
					await createPost<true>({
						path: { threadId },
						body: { content: form.data.content },
						auth: currentUser.user?.token
					})
				).data
				form.data.content = ''
				await navigateToPost(postId)
			}
		}
	})

	const updatePostForm = superForm(defaults(valibot(vUpdatePostRequestBody)), {
		SPA: true,
		validators: valibot(vUpdatePostRequestBody),
		async onUpdate({ form }) {
			if (form.valid && editedPost) {
				const postId = editedPost.postId
				await updatePost<true>({
					path: { postId },
					body: { content: form.data.content, rowVersion: editedPost.rowVersion },
					auth: currentUser.user?.token
				})
				form.data.content = ''
				await navigateToPost(postId)
				clearEdit()
			}
		}
	})

	const { form: createPostFormData, enhance: createPostFormEnhance } = createPostForm
	const { form: updatePostFormData, enhance: updatePostFormEnhance } = updatePostForm

	$effect(() => {
		if (editedPost) {
			$updatePostFormData.content = editedPost.content
			setTimeout(() => {
				const editor = document.getElementById('post-editor')
				editor?.focus({ preventScroll: true })
				editor?.scrollIntoView({ behavior: 'smooth' })
			}, 0)
		}
	})
</script>

{#if !editedPost}
	<form method="POST" use:createPostFormEnhance>
		<Form.Field form={createPostForm} name="content">
			<Form.Control>
				{#snippet children({ props })}
					<Textarea
						{...props}
						id="post-editor"
						class="bg-muted/40 sm:bg-muted/0 mt-4 h-64 w-full border-0 sm:border"
						placeholder="Type your message here."
						bind:value={$createPostFormData.content}
					/>
				{/snippet}
			</Form.Control>
			<Form.FieldErrors />
		</Form.Field>

		<div class="flex gap-2 px-4 sm:px-0">
			<Form.Button class="ml-auto mt-4">Send</Form.Button>
		</div>
	</form>
{:else}
	<form method="POST" use:updatePostFormEnhance>
		<Form.Field form={updatePostForm} name="content">
			<Form.Control>
				{#snippet children({ props })}
					<Textarea
						{...props}
						id="post-editor"
						class="bg-muted/40 sm:bg-muted/0 mt-4 h-64 w-full border-0 sm:border"
						placeholder="Type your message here."
						bind:value={$updatePostFormData.content}
					/>
				{/snippet}
			</Form.Control>
			<Form.FieldErrors />
		</Form.Field>

		<div class="flex gap-2 px-4 sm:px-0">
			<Form.Button class="ml-auto mt-4" variant="destructive" onclick={clearEdit}
				>Cancel</Form.Button
			>
			<Form.Button class="mt-4">Update</Form.Button>
		</div>
	</form>
{/if}
