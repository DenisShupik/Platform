import { writable } from 'svelte/store'

import type { Post, Thread } from '$lib/utils/client'

type IdType = Pick<Thread, 'threadId'> & Pick<Post, 'postId'>
type MapType = Map<IdType, Post>

export const postStore = writable<MapType>(new Map())
