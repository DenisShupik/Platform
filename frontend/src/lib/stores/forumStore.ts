import { writable } from 'svelte/store'

import type { Forum } from '$lib/utils/client'

type IdType = Forum['forumId']
type MapType = Map<IdType, Forum | null>

export const forumStore = writable<MapType>(new Map())
