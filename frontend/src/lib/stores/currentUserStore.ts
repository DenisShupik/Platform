import { writable } from 'svelte/store'

export interface User {
  username: string
}

export const currentUserStore = writable<User | undefined>()

export function updateUser(user?: User): void {
  currentUserStore.set(user)
}
