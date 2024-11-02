import { writable, type Writable } from 'svelte/store'
import type { Topic } from '$lib/types/Topic'

type IdType = Topic["topicId"]
type MapType = Map<IdType, Topic>

export const topicStore: Writable<MapType> = writable(new Map());
