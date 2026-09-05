namespace ERPPlatform;

public static class ERPPlatformDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */
}

/// <summary>Error codes surfaced by the resumable employee Excel import.</summary>
public static class EmployeeImportErrorCodes
{
    public const string Prefix = "ERPPlatform.EmployeeImport";

    public const string InvalidFileType = Prefix + ":InvalidFileType";
    public const string FileTooLarge = Prefix + ":FileTooLarge";
    public const string EmptyFile = Prefix + ":EmptyFile";
    public const string TooManyRows = Prefix + ":TooManyRows";
    public const string InvalidWorkbook = Prefix + ":InvalidWorkbook";
    public const string MissingColumns = Prefix + ":MissingColumns";
    public const string SourceFileMissing = Prefix + ":SourceFileMissing";
    public const string ChunkFailed = Prefix + ":ChunkFailed";
    public const string JobNotFound = Prefix + ":JobNotFound";
    public const string NotRetryable = Prefix + ":NotRetryable";
    public const string NotCancellable = Prefix + ":NotCancellable";
    public const string DuplicateSubmission = Prefix + ":DuplicateSubmission";
}
