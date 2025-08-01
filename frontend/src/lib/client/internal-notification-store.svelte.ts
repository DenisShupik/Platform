import {
	ChannelType,
	GetInternalUserNotificationQuerySortEnum,
	getUserNotification,
	getUserNotificationCount,
	type InternalUserNotificationsDto
} from '$lib/utils/client'
import { writable } from 'svelte/store'
import { currentUser } from './current-user-state.svelte'

function createStore() {
	const { subscribe, update } = writable<
		{
			count: number
		} & InternalUserNotificationsDto
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
				const result = await getUserNotification<true>({
					query: {
						isDelivered: false,
						sort: [GetInternalUserNotificationQuerySortEnum.OCCURRED_AT_ASC]
					},
					auth: currentUser.user?.token
				})
				update((state) => {
					Object.assign(state, result.data)
					return state
				})
			} catch (error) {
				console.error('Ошибка при получении уведомлений:', error)
			}
		}
	}
}

export const internalNotificationStore = createStore()
