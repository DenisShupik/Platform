import {
	AuthorizationScopeType,
	type CategoryId,
	type ForumId,
	type ThreadId
} from '$lib/utils/client'

export type AuthorizationScopeSelection =
	| {
			scopeType: AuthorizationScopeType.PLATFORM
			forumId: null
			categoryId: null
			threadId: null
	  }
	| {
			scopeType: AuthorizationScopeType.FORUM
			forumId: ForumId
			categoryId: null
			threadId: null
	  }
	| {
			scopeType: AuthorizationScopeType.CATEGORY
			forumId: ForumId
			categoryId: CategoryId
			threadId: null
	  }
	| {
			scopeType: AuthorizationScopeType.THREAD
			forumId: ForumId
			categoryId: CategoryId
			threadId: ThreadId
	  }

export const platformAuthorizationScope: AuthorizationScopeSelection = {
	scopeType: AuthorizationScopeType.PLATFORM,
	forumId: null,
	categoryId: null,
	threadId: null
}

export function toAuthorizationScopeQuery(selection: AuthorizationScopeSelection) {
	switch (selection.scopeType) {
		case AuthorizationScopeType.PLATFORM:
			return { scopeType: selection.scopeType }
		case AuthorizationScopeType.FORUM:
			return { scopeType: selection.scopeType, forumId: selection.forumId }
		case AuthorizationScopeType.CATEGORY:
			return { scopeType: selection.scopeType, categoryId: selection.categoryId }
		case AuthorizationScopeType.THREAD:
			return { scopeType: selection.scopeType, threadId: selection.threadId }
	}
}

export function toAuthorizationScopeBody(selection: AuthorizationScopeSelection) {
	return {
		scopeType: selection.scopeType,
		forumId: selection.scopeType === AuthorizationScopeType.FORUM ? selection.forumId : null,
		categoryId:
			selection.scopeType === AuthorizationScopeType.CATEGORY ? selection.categoryId : null,
		threadId: selection.scopeType === AuthorizationScopeType.THREAD ? selection.threadId : null
	}
}
