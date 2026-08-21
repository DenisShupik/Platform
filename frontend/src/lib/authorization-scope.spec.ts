import { describe, expect, it } from 'vitest'
import {
	toAuthorizationScopeBody,
	toAuthorizationScopeQuery,
	type AuthorizationScopeSelection
} from './authorization-scope'
import {
	AuthorizationScopeType,
	type CategoryId,
	type ForumId,
	type ThreadId
} from './utils/client'

const forumId = '00000000-0000-0000-0000-000000000001' as ForumId
const categoryId = '00000000-0000-0000-0000-000000000002' as CategoryId
const threadId = '00000000-0000-0000-0000-000000000003' as ThreadId

describe('authorization scope serialization', () => {
	it('sends only the leaf identifier in query parameters', () => {
		const scope: AuthorizationScopeSelection = {
			scopeType: AuthorizationScopeType.THREAD,
			forumId,
			categoryId,
			threadId
		}

		expect(toAuthorizationScopeQuery(scope)).toEqual({
			scopeType: AuthorizationScopeType.THREAD,
			threadId
		})
	})

	it('uses explicit nulls for non-applicable request body identifiers', () => {
		const scope: AuthorizationScopeSelection = {
			scopeType: AuthorizationScopeType.CATEGORY,
			forumId,
			categoryId,
			threadId: null
		}

		expect(toAuthorizationScopeBody(scope)).toEqual({
			scopeType: AuthorizationScopeType.CATEGORY,
			forumId: null,
			categoryId,
			threadId: null
		})
	})
})
