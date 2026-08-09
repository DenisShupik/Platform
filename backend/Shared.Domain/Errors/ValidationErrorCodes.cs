namespace Shared.Domain.Errors;

public static class ValidationErrorCodes
{
    public const string CannotParseEmptyValue = "cannot_parse_empty_value";
    public const string CannotParseInputValue = "cannot_parse_input_value";
    public const string CollectionMustNotBeEmpty = "collection_must_not_be_empty";
    public const string InvalidIdentifier = "invalid_identifier";
    public const string InvalidJsonBody = "invalid_json_body";
    public const string InvalidRouteValue = "invalid_route_value";
    public const string InvalidValue = "invalid_value";
    public const string MustBeNonNegative = "must_be_non_negative";
    public const string MustBeWithinInclusiveRange = "must_be_within_inclusive_range";
    public const string MustNotBeEmpty = "must_not_be_empty";
    public const string RequiredQueryValue = "required_query_value";
    public const string RequiredRouteValue = "required_route_value";
    public const string StringExceedsMaximumLength = "string_exceeds_maximum_length";
    public const string StringIsShorterThanMinimumLength = "string_is_shorter_than_minimum_length";
    public const string UnsupportedLocale = "unsupported_locale";
    public const string UsernameHasInvalidFormat = "username_has_invalid_format";
    public const string ValidationRuleFailed = "validation_rule_failed";
}
