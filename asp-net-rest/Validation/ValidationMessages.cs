namespace AspNetRest.Validation;

public static class ValidationMessages
{
    public const string TitleField = "title";
    public const string AuthorField = "author";
    public const string YearField = "year";
    public const string SortByField = "sortBy";
    public const string PageField = "page";
    public const string PageSizeField = "pageSize";
    public const string RequestField = "request";

    public const string TitleRequired = "The title must not be empty.";
    public const string TitleTooLong = "The title must not exceed {MaxLength} characters.";
    public const string AuthorRequired = "The author must not be empty.";
    public const string AuthorTooLong = "The author name must not exceed {MaxLength} characters.";
    public const string YearOutOfRange = "The publication year must be between {From} and {To}.";
    public const string DuplicateEdition = "This book by this author has already been recorded.";
    public const string BodyRequired = "No usable request content was sent.";
    public const string PatchWithoutChange = "At least one field must be set.";
    public const string PageTooSmall = "Page numbers start at 1.";
    public const string PageSizeOutOfRange = "The page size must be between 1 and {To}.";
    public const string UnknownSortKey = "Unknown sort key '{PropertyValue}'. Allowed values: {AllowedKeys}.";
    public const string OptionMustBePositive = "'{PropertyName}' must be greater than 0.";
    public const string DefaultPageSizeTooLarge = "DefaultPageSize must not exceed MaxPageSize.";

    public const string AllowedKeysPlaceholder = "{AllowedKeys}";
}
