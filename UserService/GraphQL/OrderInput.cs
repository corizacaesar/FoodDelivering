namespace UserService.GraphQL
{
    public record OrderInput
    (
        int OrderId,
        int CourierId,
        bool Status
    );
}
