namespace Atc.LogAnalytics;

/// <summary>
/// Abstract base record for Log Analytics scripts that loads KQL from embedded resources.
/// </summary>
public abstract record LogAnalyticsScript : ILogAnalyticsScript
{
    private static readonly ConcurrentDictionary<Type, string> ScriptCache = new();

    /// <inheritdoc />
    public string Script => GetOrLoadScript();

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Parameters => GetParameters();

    /// <summary>
    /// Gets the file extension for the script file.
    /// </summary>
    protected virtual string FileExtension => ".kql";

    private string GetOrLoadScript()
        => ScriptCache.GetOrAdd(GetType(), LoadScriptFromResource);

    private string LoadScriptFromResource(Type type)
    {
        var assembly = type.Assembly;
        var resourceName = FindResourceName(assembly, type);

        if (resourceName is null)
        {
            throw new InvalidOperationException(
                $"Could not find embedded resource '{type.Name}{FileExtension}' for type '{type.FullName}'. " +
                $"Ensure the .kql file is marked as an embedded resource and has the same name as the class.");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new InvalidOperationException(
                $"Could not load embedded resource '{resourceName}' from assembly '{assembly.FullName}'.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private string? FindResourceName(
        Assembly assembly,
        Type type)
    {
        var resourceNames = assembly.GetManifestResourceNames();
        var expectedFileName = $"{type.Name}{FileExtension}";

        // First try exact match with namespace
        var namespacedName = $"{type.Namespace}.{expectedFileName}";
        if (resourceNames.Contains(namespacedName))
        {
            return namespacedName;
        }

        // Then try to find by file name only
        return resourceNames.FirstOrDefault(x => x.EndsWith(expectedFileName, StringComparison.OrdinalIgnoreCase));
    }

    private Dictionary<string, object?> GetParameters()
    {
        var type = GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var parameters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in properties)
        {
            // Skip inherited properties from base record types
            if (property.DeclaringType == typeof(LogAnalyticsScript) ||
                property.DeclaringType == typeof(object))
            {
                continue;
            }

            // Skip the Script and Parameters properties
            if (property.Name is nameof(Script) or nameof(Parameters) or nameof(FileExtension))
            {
                continue;
            }

            var value = property.GetValue(this);
            var paramName = property.Name.ToCamelCase();
            parameters[paramName] = value;
        }

        return parameters;
    }
}