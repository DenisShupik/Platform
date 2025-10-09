import { type InternalNotificationsPagedDto } from '$lib/utils/client'
import { getInternalNotificationsPagedResponseTransformer } from '$lib/utils/client/transformers.gen'
import { writable } from 'svelte/store'

function createStore() {
	const { subscribe, update } = writable<
		{
			count: number
		} & InternalNotificationsPagedDto
	>({
		count: 0,
		notifications: [],
		users: {},
		threads: {},
		totalCount: 0n
	})

	return {
		subscribe,

		async update() {
			try {
				const response = await fetch('/api/notifications', {
					method: 'GET',
					credentials: 'include'
				})
				if (!response.ok) {
					throw new Error(`HTTP error! status: ${response.status}`)
				}
				let result = await response.json()
				result = await getInternalNotificationsPagedResponseTransformer(result)
				update((state) => {
					Object.assign(state, result)
					return state
				})
			} catch (error) {
				console.error('Ошибка при получении уведомлений:', error)
			}
		}
	}
}

export const internalNotificationStore = createStore()
