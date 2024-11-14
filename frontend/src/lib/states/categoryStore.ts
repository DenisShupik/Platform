import { writable } from 'svelte/store'

import type { Category } from '$lib/utils/client'

type IdType = Category['categoryId']
type MapType = Map<IdType, Category | null>

export const categoryStore = writable<MapType>(new Map())
