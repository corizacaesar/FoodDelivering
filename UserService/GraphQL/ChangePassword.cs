namespace UserService.GraphQL
{
    public record ChangePassword
    (
        string CurrentPassword,
        string NewPassword,
        string? ConfirmPassword
    );
}
