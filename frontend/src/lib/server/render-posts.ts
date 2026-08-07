import { createPostCarta } from '$lib/markdown/carta'
import type { PostDto } from '$lib/utils/client'

export type RenderedPost = PostDto & { renderedContent: string }

/** Renders Markdown for an SSR response; rendered HTML is never persisted. */
export const renderPosts = (posts: readonly PostDto[]): RenderedPost[] => {
	const carta = createPostCarta()
	return posts.map((post) => ({
		...post,
		renderedContent: carta.renderSSR(post.content)
	}))
}
