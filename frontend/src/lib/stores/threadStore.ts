import { writable } from 'svelte/store'

import type { Thread } from '$lib/utils/client'

type IdType = Thread['threadId']
type MapType = Map<IdType, Thread | null>

export const threadStore = writable<MapType>(new Map())
