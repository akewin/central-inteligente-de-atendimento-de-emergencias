using System;
using System.Collections.Generic;

namespace Api.Security.Permissions;

public static class EndpointPermissions
{
    // Chave: rota ou padrão da rota
    // Valor: ID do SystemResource necessário para acessar
    public static readonly Dictionary<string, int[]> Rules = new()
        {
            { "/users", new[] { 2 } },

            { "/resources", new[] { 3 } },

            { "/reports", new[] { 4 } },

            { "/occurrences", new[] { 5 } },

            // Outros endpoints podem ser adicionados aqui
            // { "/outro-endpoint", new[] { id } }
        };

    public static int[] GetRequiredPermissions(string path)
    {
        path = path.ToLower();
        foreach (var rule in Rules)
        {
            if (path.Contains(rule.Key))
                return rule.Value;
        }

        return Array.Empty<int>();
    }

    public static int[] GetRequiredPermissions(string path, string method)
    {
        path = path.ToLowerInvariant();
        method = method.ToUpperInvariant();

        if (path.Contains("/hubs/dispatches"))
            return new[] { BasePermissions.UNIT_OPERATIONS, BasePermissions.HOSPITAL_OPERATIONS, BasePermissions.OCCURRENCES };

        if (path.Contains("/unit-operations"))
            return new[] { BasePermissions.UNIT_OPERATIONS };

        if (path.Contains("/hospital-receptions"))
            return new[] { BasePermissions.HOSPITAL_OPERATIONS };

        if (path.Contains("/hospitals"))
            return method == "GET"
                ? new[] { BasePermissions.HOSPITALS_VIEW, BasePermissions.HOSPITALS_MANAGE, BasePermissions.UNIT_OPERATIONS, BasePermissions.USERS }
                : new[] { BasePermissions.HOSPITALS_MANAGE };

        if (path.Contains("/units") || path.Contains("/emergency-services"))
        {
            return method == "GET"
                ? new[] { BasePermissions.UNITS_VIEW, BasePermissions.UNITS_MANAGE }
                : new[] { BasePermissions.UNITS_MANAGE };
        }

        return GetRequiredPermissions(path);
    }
}

