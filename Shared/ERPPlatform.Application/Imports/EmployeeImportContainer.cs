using Volo.Abp.BlobStoring;

namespace ERPPlatform.Application.Imports;

/// <summary>
/// Isolated blob container for retained employee-import spreadsheets. The file must
/// survive until every chunk is done (each chunk re-reads its own row range), so it
/// is only removed once the job reaches a terminal state.
/// </summary>
[BlobContainerName(EmployeeImportConsts.BlobContainerName)]
public class EmployeeImportContainer
{
}
