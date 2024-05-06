namespace ManagerBack.Validators;

/// <summary>
/// Validator interface
/// </summary>
/// <typeparam name="T">Type of validated object</typeparam>
public interface IValidator<T> {
    /// <summary>
    /// Validates the object
    /// </summary>
    /// <param name="value">Object to be validated</param>
    public Task Validate(T value);
}