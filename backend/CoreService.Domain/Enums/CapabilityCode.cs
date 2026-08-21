namespace CoreService.Domain.Enums;

/// <summary>
/// Стабильный числовой код атомарного полномочия. Значения append-only.
/// </summary>
public enum CapabilityCode : short
{
    ManageStructure = 1,
    ViewUnpublishedThreads = 2,
    ApproveThreads = 3,
    RejectThreads = 4,
    EditAnyPost = 5,
    DeleteAnyPost = 6,
    ManageAuthorization = 7,
    ManageSanctions = 8
}
