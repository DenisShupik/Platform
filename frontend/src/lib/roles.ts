export const Role = {
	User: 'User',
	Moderator: 'Moderator',
	Administrator: 'Administrator'
} as const

export type Role = (typeof Role)[keyof typeof Role]

const ROLE_INDEX = {
	User: 0,
	Moderator: 1,
	Administrator: 2
} as const satisfies Record<Role, number>

export function getEffectiveRole(roleValue: unknown): Role | null {
	if (!Array.isArray(roleValue)) return null

	const parsedRoles = roleValue
		.filter((x): x is string => typeof x === 'string')
		.map((x) => x.trim())
		.filter((x): x is Role => x in ROLE_INDEX)

	return parsedRoles.length ? parsedRoles.sort((a, b) => ROLE_INDEX[b] - ROLE_INDEX[a])[0] : null
}

export function roleAtLeast(role: Role | undefined, required: Role): boolean {
	return role ? ROLE_INDEX[role] >= ROLE_INDEX[required] : false
}

export function canCreateForumPolicy(role?: Role) {
	return roleAtLeast(role, Role.Moderator)
}

export function canCreateCategoryPolicy(role?: Role) {
	return roleAtLeast(role, Role.Moderator)
}

export function canCreateThreadPolicy(role?: Role) {
	return roleAtLeast(role, Role.User)
}
