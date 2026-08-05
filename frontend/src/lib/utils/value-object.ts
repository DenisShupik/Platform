import {
	vCategoryId,
	vCategoryTitle,
	vCount,
	vForumId,
	vForumTitle,
	vIndex,
	vPaginationLimitMin10Max100,
	vPaginationOffset,
	vPostContent,
	vPostId,
	vSearchTerm,
	vThreadId,
	vThreadTitle,
	vUserId
} from '$lib/utils/client/valibot.gen'
import type {
	CategoryId,
	CategoryTitle,
	Count,
	ForumId,
	ForumTitle,
	Index,
	PaginationLimitMin10Max100,
	PaginationOffset,
	PostContent,
	PostId,
	SearchTerm,
	ThreadId,
	ThreadTitle,
	UserId
} from '$lib/utils/client'
import { safeParse } from 'valibot'

/** Branded values can only be created after their generated Valibot schema succeeds. */
export const parseCategoryId = (value: unknown): CategoryId | undefined => {
	const result = safeParse(vCategoryId, value)
	return result.success ? (result.output as CategoryId) : undefined
}

export const parseCategoryTitle = (value: unknown): CategoryTitle | undefined => {
	const result = safeParse(vCategoryTitle, value)
	return result.success ? (result.output as CategoryTitle) : undefined
}

export const parseCount = (value: unknown): Count | undefined => {
	const result = safeParse(vCount, value)
	return result.success ? (result.output as Count) : undefined
}

export const parseForumId = (value: unknown): ForumId | undefined => {
	const result = safeParse(vForumId, value)
	return result.success ? (result.output as ForumId) : undefined
}

export const parseForumTitle = (value: unknown): ForumTitle | undefined => {
	const result = safeParse(vForumTitle, value)
	return result.success ? (result.output as ForumTitle) : undefined
}

export const parseIndex = (value: unknown): Index | undefined => {
	const result = safeParse(vIndex, value)
	return result.success ? (result.output as Index) : undefined
}

export const parsePaginationLimit = (value: unknown): PaginationLimitMin10Max100 | undefined => {
	const result = safeParse(vPaginationLimitMin10Max100, value)
	return result.success ? (result.output as PaginationLimitMin10Max100) : undefined
}

export const parsePaginationOffset = (value: unknown): PaginationOffset | undefined => {
	const result = safeParse(vPaginationOffset, value)
	return result.success && result.output !== undefined
		? (result.output as PaginationOffset)
		: undefined
}

export const parsePostContent = (value: unknown): PostContent | undefined => {
	const result = safeParse(vPostContent, value)
	return result.success ? (result.output as PostContent) : undefined
}

export const parsePostId = (value: unknown): PostId | undefined => {
	const result = safeParse(vPostId, value)
	return result.success ? (result.output as PostId) : undefined
}

export const parseSearchTerm = (value: unknown): SearchTerm | undefined => {
	const result = safeParse(vSearchTerm, value)
	return result.success ? (result.output as SearchTerm) : undefined
}

export const parseThreadId = (value: unknown): ThreadId | undefined => {
	const result = safeParse(vThreadId, value)
	return result.success ? (result.output as ThreadId) : undefined
}

export const parseThreadTitle = (value: unknown): ThreadTitle | undefined => {
	const result = safeParse(vThreadTitle, value)
	return result.success ? (result.output as ThreadTitle) : undefined
}

export const parseUserId = (value: unknown): UserId | undefined => {
	const result = safeParse(vUserId, value)
	return result.success ? (result.output as UserId) : undefined
}

function validOrThrow<T>(value: T | undefined, name: string): T {
	if (value === undefined) throw new Error(`Invalid built-in ${name} value`)
	return value
}

export const zeroCount = validOrThrow(parseCount(0), 'Count')
export const defaultPaginationLimit = validOrThrow(
	parsePaginationLimit(10),
	'PaginationLimitMin10Max100'
)

export function createPagination(currentPage: number, perPage: number) {
	const offset = parsePaginationOffset((currentPage - 1) * perPage)
	const limit = parsePaginationLimit(perPage)

	return {
		offset: validOrThrow(offset, 'PaginationOffset'),
		limit: validOrThrow(limit, 'PaginationLimitMin10Max100')
	}
}

export const createIndex = (value: number): Index => validOrThrow(parseIndex(value), 'Index')
