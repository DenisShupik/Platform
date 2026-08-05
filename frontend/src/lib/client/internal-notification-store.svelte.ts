import {
	getInternalNotificationsPaged,
	GetInternalNotificationsPagedQuerySortType,
	type Count,
	type InternalNotificationsPagedDto
} from '$lib/utils/client'
import { zeroCount } from '$lib/utils/value-object'
import { writable } from 'svelte/store'

function createStore() {
	const { subscribe, update } = writable<
		{
			count: Count
		} & InternalNotificationsPagedDto
	>({
		count: zeroCount,
		notifications: [],
		users: {},
		threads: {},
		totalCount: zeroCount
	})

	return {
		subscribe,

		async update() {
			try {
				const result = (
					await getInternalNotificationsPaged({
						query: {
							isDelivered: false,
							sort: [GetInternalNotificationsPagedQuerySortType.OCCURRED_AT_ASC]
						}
					})
				).data
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
