import type { InternalNotificationsPagedDto } from '$lib/utils/client'

export type NotificationReferences = Pick<InternalNotificationsPagedDto, 'threads' | 'users'>
