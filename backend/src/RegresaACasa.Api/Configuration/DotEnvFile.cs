namespace RegresaACasa.Api.Configuration;

/// <summary>
/// Carga un archivo .env (KEY=VALUE) como variables de entorno antes de construir la app.
/// Busca desde el directorio actual hacia arriba (p. ej. backend/.env) y nunca
/// sobrescribe variables que ya existan en el sistema.
/// </summary>
public static class DotEnvFile
{
    public static string? Load(string startDirectory)
    {
        var path = Find(startDirectory);
        if (path is null)
        {
            return null;
        }

        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim().Trim('"');
            if (Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }

        return path;
    }

    private static string? Find(string startDirectory)
    {
        for (var dir = new DirectoryInfo(startDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            // No salir del repositorio.
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            {
                return null;
            }
        }

        return null;
    }
}
