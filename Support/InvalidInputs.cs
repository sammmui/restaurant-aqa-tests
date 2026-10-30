namespace Restaurant_Aqa_Framework.Support;

/// <summary>The form fields a visitor can get wrong, named as they appear to a person.</summary>
public enum InputField
{
    FirstName,
    LastName,
    EmailAddress,
    Password,
    PasswordConfirmation,
}

/// <summary>A named registration value, invalid or permitted, in the words feature files use.</summary>
public enum InputProblem
{
    TooShort,
    TooLong,
    Empty,
    WithDigits,
    WithAnUnderscore,
    WithASymbol,
    WithANonLatinLetter,
    WithHyphen,
    WithApostrophe,
    WithoutAtSign,
    WithoutDomain,
    DifferentFromPassword,
}

public enum InputBoundary
{
    MinimumLength,
    MaximumLength,
}

/// <summary>What is wrong with a location a manager tries to set up.</summary>
public enum LocationProblem
{
    EmptyAddress,
    NegativeCapacity,
}

/// <summary>
/// The concrete registration values behind phrases such as "too long" and "with a hyphen". Feature
/// files stay in business language; exact characters and lengths live here, in one place.
/// </summary>
public static class InvalidInputs
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 50;
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 16;
    private const int MaxEmailLength = 254;

    public static string Value(InputField field, InputProblem problem) => (field, problem) switch
    {
        (InputField.FirstName or InputField.LastName, InputProblem.TooShort) => "A",
        (InputField.FirstName or InputField.LastName, InputProblem.TooLong) => new string('a', MaxNameLength + 1),
        (InputField.FirstName or InputField.LastName, InputProblem.WithDigits) => "Ab1",
        (InputField.FirstName or InputField.LastName, InputProblem.WithAnUnderscore) => "Te_st",
        (InputField.FirstName or InputField.LastName, InputProblem.WithASymbol) => "Te!st",
        (InputField.FirstName or InputField.LastName, InputProblem.WithANonLatinLetter) => "Test\u0416",
        (InputField.FirstName or InputField.LastName, InputProblem.WithHyphen) => "Anne-Marie",
        (InputField.FirstName or InputField.LastName, InputProblem.WithApostrophe) => "O'Neil",
        (InputField.EmailAddress, InputProblem.WithoutAtSign) => "not-an-email",
        (InputField.EmailAddress, InputProblem.WithoutDomain) => "someone@nodot",
        (InputField.EmailAddress, InputProblem.TooLong) => EmailWithLength(MaxEmailLength + 1, null),
        (InputField.Password, InputProblem.TooShort) => "Aa1!xxx",
        (InputField.Password, InputProblem.TooLong) => "Aa1!xxxxxxxxxxxxx",
        (InputField.PasswordConfirmation, InputProblem.DifferentFromPassword) => "Different1!",
        (_, InputProblem.Empty) => "",
        _ => throw new NotSupportedException($"No invalid value is defined for '{field}' that is '{problem}'."),
    };

    public static string BoundaryValue(InputField field, InputBoundary boundary, string? uniqueEmail = null) =>
        (field, boundary) switch
        {
            (InputField.FirstName or InputField.LastName, InputBoundary.MinimumLength) => new string('a', MinNameLength),
            (InputField.FirstName or InputField.LastName, InputBoundary.MaximumLength) => new string('a', MaxNameLength),
            (InputField.EmailAddress, InputBoundary.MaximumLength) => EmailWithLength(MaxEmailLength, uniqueEmail),
            (InputField.Password, InputBoundary.MinimumLength) => PasswordOfLength(MinPasswordLength),
            (InputField.Password, InputBoundary.MaximumLength) => PasswordOfLength(MaxPasswordLength),
            _ => throw new NotSupportedException($"No accepted boundary is defined for '{field}' at '{boundary}'."),
        };

    private static string PasswordOfLength(int length) => $"Aa1!{new string('x', length - 4)}";

    private static string EmailWithLength(int length, string? uniqueEmail)
    {
        var localPart = uniqueEmail is null
            ? new string('a', 64)
            : uniqueEmail[..uniqueEmail.IndexOf('@')].PadRight(64, 'a');
        var domainLength = length - localPart.Length - 1;
        var firstLabelLength = domainLength % 4;
        var repeatedLabels = (domainLength - firstLabelLength) / 4;

        return $"{localPart}@{new string('b', firstLabelLength)}{string.Concat(Enumerable.Repeat(".abc", repeatedLabels))}";
    }
}
