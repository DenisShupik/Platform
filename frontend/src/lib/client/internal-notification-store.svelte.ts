import {
	ChannelType,
	getUserNotificationCount,
	type InternalUserNotificationDto
} from '$lib/utils/client'
import { writable } from 'svelte/store'

function createState() {
	const { subscribe, update } = writable<{
		count: number
		notifications: InternalUserNotificationDto[]
	}>({
		count: 0,
		notifications: []
	})

	return {
		subscribe,

		async fetchCount(token: string) {
			try {
				const result = await getUserNotificationCount<true>({
					query: { isDelivered: false, channel: ChannelType.INTERNAL },
					auth: token
				})
				update((state) => ({ ...state, count: result.data }))
			} catch (error) {
				console.error('Ошибка при получении количества уведомлений:', error)
			}
		}
	}
}

// экспортируем только то, что хотим дать наружу
export const InternalNotificationStore = createState()
