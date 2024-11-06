import { writable } from 'svelte/store'
import type { Topic } from '$lib/types/Topic'

type IdType = Topic['topicId']
type MapType = Map<IdType, Topic>

export const topicStore = writable<MapType>(new Map())
