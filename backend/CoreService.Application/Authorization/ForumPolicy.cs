namespace CoreService.Application.Authorization;

/// <summary>
/// Политики именуют бизнес-действия; они не являются ролями и не хранятся в токене.
/// </summary>
public enum ForumPolicy : byte
{
    ManageStructure = 1,
    ViewUnpublishedThreads = 2,
    ApproveThread = 3,
    RejectThread = 4,
    EditAnyPost = 5,
    DeleteAnyPost = 6,
    ManageAuthorization = 7,
    ManageSanctions = 8
}
