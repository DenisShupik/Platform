import { error } from '@sveltejs/kit'
import { withApiLocale } from '$lib/client/api-options'
import { getSuccessfulResultMap } from '$lib/utils/result'
import {
	getPlatformAdministrators,
	getUsersBulk,
	type UserDto,
	type UserId
} from '$lib/utils/client'
import type { PageServerLoad } from './$types'

export const load: PageServerLoad = async ({ locals, parent }) => {
	const auth = locals.accessToken
	const { platformAllowedActions, administrationAllowedActions } = await parent()
	if (
		!auth ||
		(!administrationAllowedActions.canManageAnyAuthorization &&
			!administrationAllowedActions.canManageAnySanctions)
	)
		error(403)

	const appointments = administrationAllowedActions.canManagePlatformAuthorization
		? (await getPlatformAdministrators<true>(withApiLocale({ auth, throwOnError: true }))).data
		: []
	const userIds = new Set(
		appointments.flatMap((appointment) =>
			appointment.grantedBy ? [appointment.userId, appointment.grantedBy] : [appointment.userId]
		)
	)
	const users: Map<UserId, UserDto> = userIds.size
		? getSuccessfulResultMap(
				(
					await getUsersBulk<true>(
						withApiLocale({
							path: { userIds: [...userIds] },
							auth,
							throwOnError: true
						})
					)
				).data
			)
		: new Map()

	return {
		allowedActions: administrationAllowedActions,
		platformAllowedActions,
		appointments,
		users
	}
}
