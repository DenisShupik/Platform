import {
	getInternalNotificationsPaged,
	getInternalNotificationCount,
	GetInternalNotificationsPagedQuerySortType,
	type Count,
	type InternalNotificationsPagedDto
} from '$lib/utils/client'
import { zeroCount } from '$lib/utils/value-object'

function emptyNotifications(): InternalNotificationsPagedDto {
	return {
		notifications: [],
		users: {},
		threads: {},
		totalCount: zeroCount
	}
}

class InternalNotificationStore {
	#notifications = $state.raw<InternalNotificationsPagedDto>(emptyNotifications())
	#unreadCount = $state<Count>(zeroCount)
	#revision = 0

	get notifications() {
		return this.#notifications.notifications
	}

	get users() {
		return this.#notifications.users
	}

	get threads() {
		return this.#notifications.threads
	}

	get unreadCount() {
		return this.#unreadCount
	}

	reset() {
		this.#revision += 1
		this.#notifications = emptyNotifications()
		this.#unreadCount = zeroCount
	}

	async refreshUnreadCount(signal?: AbortSignal) {
		const revision = this.#revision

		try {
			const result = (
				await getInternalNotificationCount<true>({
					query: { isDelivered: false },
					signal
				})
			).data

			if (!signal?.aborted && revision === this.#revision) this.#unreadCount = result
		} catch (error) {
			if (!signal?.aborted) console.error('Failed to load notification count:', error)
		}
	}

	async update(signal?: AbortSignal) {
		const revision = this.#revision

		try {
			const result = (
				await getInternalNotificationsPaged<true>({
					query: {
						isDelivered: false,
						sort: [GetInternalNotificationsPagedQuerySortType.OCCURRED_AT_ASC]
					},
					signal
				})
			).data

			if (signal?.aborted || revision !== this.#revision) return

			this.#notifications = result
			this.#unreadCount = result.totalCount
		} catch (error) {
			if (!signal?.aborted) console.error('Failed to load notifications:', error)
		}
	}
}

export const internalNotificationStore = new InternalNotificationStore()
