namespace Api.Auditing;

public static class SystemLogActionFactory
{
    public static string Create(string entity, int id)
        => $"create {entity} id: {id}";

    public static string Update(string entity, int id)
        => $"update {entity} id: {id}";

    public static string Delete(string entity, int id)
        => $"delete {entity} id: {id}";

    public static string Login(string username)
        => $"user {username} fez login no sistema";

    public static string ExternalLogin(string username)
        => $"user {username} fez login por redirecionamento no sistema";

    public static string Logout(string username)
        => $"user {username} fez logout do sistema";

    public static string NewPasswordRequest(string username)
        => $"user {username} solicitou reset de senha";

    public static string PasswordReset(string username)
        => $"user {username} alterou a senha";

    public static string TokenRefreshed(string username)
        => $"user {username} renovou o token";
}
