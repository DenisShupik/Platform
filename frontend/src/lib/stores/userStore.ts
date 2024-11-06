import { writable } from 'svelte/store'
import type { User } from '$lib/types/User'

type IdType = User['userId']
type MapType = Map<IdType, User>

export const userStore = writable<MapType>(new Map())
