import { writable } from 'svelte/store'

import type { Section } from '$lib/types/Section'

type IdType = Section['sectionId']
type MapType = Map<IdType, Section>

export const sectionStore = writable<MapType>(new Map())
