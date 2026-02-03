namespace Atc.LogAnalytics.Extensions;

/// <summary>
/// Extension methods for <see cref="LogsTable"/>.
/// </summary>
public static class LogsTableExtensions
{
    /// <summary>
    /// Reads the table rows into an array of objects of type T using a JSON intermediate step.
    /// Each row is serialized to JSON via <see cref="Utf8JsonWriter"/> and then deserialized
    /// to <typeparamref name="T"/> using <see cref="JsonSerializer"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="table">The logs table.</param>
    /// <param name="options">Optional JSON serializer options. When <c>null</c>, uses built-in defaults (case-insensitive, numbers from strings).</param>
    /// <returns>An array of deserialized objects.</returns>
    public static T[] ReadObjects<T>(
        this LogsTable table,
        JsonSerializerOptions? options = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(table);

        options ??= LogAnalyticsJsonSerializerOptions.Default;

        var columns = table.Columns.ToList();
        var results = new List<T>(table.Rows.Count);
        var buffer = new ArrayBufferWriter<byte>();

        foreach (var row in table.Rows)
        {
            buffer.Clear();

            using var writer = new Utf8JsonWriter(buffer);
            writer.WriteStartObject();

            for (var i = 0; i < columns.Count; i++)
            {
                var columnName = columns[i].Name;
                var propertyName = options.PropertyNamingPolicy?.ConvertName(columnName) ?? columnName;

                writer.WritePropertyName(propertyName);
                WriteValue(writer, row[i], options);
            }

            writer.WriteEndObject();
            writer.Flush();

            var item = JsonSerializer.Deserialize<T>(buffer.WrittenSpan, options);
            if (item is not null)
            {
                results.Add(item);
            }
        }

        return results.ToArray();
    }

    private static void WriteValue(
        Utf8JsonWriter writer,
        object? value,
        JsonSerializerOptions options)
    {
        switch (value)
        {
            case null:
            case DBNull:
                writer.WriteNullValue();
                break;
            case JsonElement jsonElement:
                jsonElement.WriteTo(writer);
                break;
            default:
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
                break;
        }
    }
}