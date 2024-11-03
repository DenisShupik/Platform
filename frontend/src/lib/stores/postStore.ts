import { writable } from 'svelte/store'
import type { Post } from '$lib/types/Post'
import type { Topic } from '$lib/types/Topic'

type IdType = Pick<Topic, 'topicId'> & Pick<Post, 'postId'>
type MapType = Map<IdType, Post>

export const postStore = writable<MapType>(new Map())
