import type {
	AdministrationAllowedActionsDto,
	CategoryAllowedActionsDto,
	ForumAllowedActionsDto,
	PlatformAllowedActionsDto,
	ThreadAllowedActionsDto
} from '$lib/utils/client'

export const noAdministrationAllowedActions: AdministrationAllowedActionsDto = {
	canManageAnyAuthorization: false,
	canManageAnySanctions: false,
	canManagePlatformAuthorization: false,
	canManagePlatformSanctions: false
}

export const noCategoryAllowedActions: CategoryAllowedActionsDto = {
	canManageStructure: false,
	canViewUnpublishedThreads: false,
	canApproveThread: false,
	canRejectThread: false,
	canEditAnyPost: false,
	canDeleteAnyPost: false,
	canManageModerators: false,
	canManageSanctions: false
}

export const noForumAllowedActions: ForumAllowedActionsDto = {
	canManageStructure: false,
	canManageAuthorization: false,
	canManageSanctions: false
}

export const noPlatformAllowedActions: PlatformAllowedActionsDto = {
	canManageStructure: false,
	canManageAuthorization: false,
	canManageSanctions: false
}

export const noThreadAllowedActions: ThreadAllowedActionsDto = {
	canViewUnpublishedThreads: false,
	canApproveThread: false,
	canRejectThread: false,
	canEditAnyPost: false,
	canDeleteAnyPost: false,
	canManageAuthorization: false,
	canManageSanctions: false
}
