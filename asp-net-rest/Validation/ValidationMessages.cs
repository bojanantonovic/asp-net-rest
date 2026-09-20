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

    public const string TitleRequired = "Der Titel darf nicht leer sein.";
    public const string TitleTooLong = "Der Titel darf höchstens {MaxLength} Zeichen lang sein.";
    public const string AuthorRequired = "Der Autor darf nicht leer sein.";
    public const string AuthorTooLong = "Der Autorenname darf höchstens {MaxLength} Zeichen lang sein.";
    public const string YearOutOfRange = "Das Erscheinungsjahr muss zwischen {From} und {To} liegen.";
    public const string DuplicateEdition = "Dieses Buch ist von diesem Autor bereits erfasst.";
    public const string BodyRequired = "Es wurde kein verwertbarer Request-Inhalt gesendet.";
    public const string PatchWithoutChange = "Mindestens ein Feld muss gesetzt sein.";
    public const string PageTooSmall = "Die Seitennummer beginnt bei 1.";
    public const string PageSizeOutOfRange = "Die Seitengrösse muss zwischen 1 und {To} liegen.";
    public const string UnknownSortKey = "Unbekannte Sortierung '{PropertyValue}'. Erlaubt sind: {AllowedKeys}.";
    public const string OptionMustBePositive = "'{PropertyName}' muss grösser als 0 sein.";
    public const string DefaultPageSizeTooLarge = "DefaultPageSize darf MaxPageSize nicht überschreiten.";

    public const string AllowedKeysPlaceholder = "{AllowedKeys}";
}
