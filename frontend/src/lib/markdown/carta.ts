import { Carta } from 'carta-md'
import DOMPurify from 'isomorphic-dompurify'

const forbiddenTags = ['embed', 'iframe', 'math', 'object', 'script', 'style', 'svg']

/** Shared Markdown policy for composing and viewing posts. */
const sanitizePostHtml = (html: string) =>
	DOMPurify.sanitize(html, {
		FORBID_ATTR: ['style'],
		FORBID_TAGS: forbiddenTags
	})

/**
 * Creates an isolated Carta instance for a component.
 *
 * Markdown remains the source of truth. The same policy is applied during SSR
 * and in the browser without sharing Carta's mutable renderer state between posts.
 */
export const createPostCarta = () => new Carta({ sanitizer: sanitizePostHtml })
