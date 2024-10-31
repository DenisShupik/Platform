import { writable, type Writable } from 'svelte/store'
import type { Category } from '$lib/types/Category';

type IdType = Category["categoryId"]
type MapType = Map<IdType, Category>

export const categoryStore: Writable<MapType> = writable(new Map());
