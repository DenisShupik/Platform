import { writable } from 'svelte/store'
import type { Category } from '$lib/types/Category'

type IdType = Category['categoryId']
type MapType = Map<IdType, Category>

export const categoryStore = writable<MapType>(new Map())
