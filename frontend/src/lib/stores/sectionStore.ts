import { writable, type Writable } from 'svelte/store'
import type { Section } from '$lib/types/Section'

type IdType = Section["sectionId"]
type MapType = Map<IdType, Section>

export const sectionStore: Writable<MapType> = writable(new Map());