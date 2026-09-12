using Api.Middlewares;

public static class Guard
{
    public static void AgainstNull(object? value, string message)
    {
        if (value is null)
            throw new AppException(message);
    }

    public static void AgainstNullOrEmpty(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new AppException($"'{field}' é obrigatório.");
    }

    public static void AgainstNonPositiveInt(int id)
    {
        if (id <= 0)
            throw new AppException("Id inválido.");
    }

    public static void AgainstMismatchedIds(int routeId, int dtoId)
    {
        if (routeId != dtoId)
            throw new AppException("Id da rota difere do id do payload.");
    }
}
