import { writable } from 'svelte/store'

export interface User {
  username: string
}

export const userStore = writable<User | undefined>()

export function updateUser(user?: User): void {
  userStore.set(user)
}
