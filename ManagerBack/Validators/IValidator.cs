namespace ManagerBack.Validators;

public interface IValidator<T> {
    public Task Validate(T value);
}