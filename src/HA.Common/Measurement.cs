using System.Globalization;
using System.Text;
using System.Text.Json;

namespace HA.Common;

public class Measurement
{
    public string? ExternalId { get; set; }

    public string? Device { get; set; }

    public QualityInfos Quality { get; set; } = QualityInfos.Good;

    public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

    public long Ticks { get; set; } = DateTime.Now.Ticks;

    public List<MeasuredValue> Values { get; set; } = new List<MeasuredValue>();

    public bool ContainsMeasuredValueKey(string key)
    {
        return Values.Any(mv => mv.Name == key);
    }

    public DateTime GetTimeStamp()
    {
        return new DateTime(Ticks);
    }

    public DateTime GetUtcTimeStamp()
    {
        return new DateTime(Ticks).ToUniversalTime();
    }

    public string ToLineProtocol()
    {
        var sb = new StringBuilder();
        //weather temperature=82 1465839830100400200
        //weather,location=us-midwest temperature=82,humidity=71 1465839830100400200
        sb.Append(LineProtocolSyntax.EscapeName(Device ?? "unknown"));
        foreach (var tagKey in Tags.Keys)
        {
            var value = Tags[tagKey];
            if (string.IsNullOrEmpty(value)) continue;
            sb.Append(',');
            sb.Append(LineProtocolSyntax.EscapeName(tagKey));
            sb.Append('=');
            sb.Append(LineProtocolSyntax.EscapeName(value));
        }
        sb.Append($",Quality={Quality.ToString()}");
        var fieldDelim = ' ';
        foreach (var value in Values)
        {
            if (value.Value == null) continue;
            sb.Append(fieldDelim);
            fieldDelim = ',';
            sb.Append(LineProtocolSyntax.EscapeName(value.Name ?? ""));
            sb.Append('=');
            sb.Append(LineProtocolSyntax.FormatValue(value.Value));
        }
        // 1669473029372994100 ns
        // 1669473029372994    µs
        // 1669473029372       ms
        // 1669473029          s
        sb.Append(" ")
          .Append(LineProtocolSyntax.FormatTimestamp(GetUtcTimeStamp()));
        return sb.ToString();
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    public static Measurement? FromJson(string measurement)
    {
        return !string.IsNullOrWhiteSpace(measurement)
            ? JsonSerializer.Deserialize<Measurement>(measurement)
            : null;
    }

    public static Measurement FromLineProtocol(string line)
    {
        var blankDelimiter = new char[] { ' ' };
        var result = new Measurement();
        var tagFollowCharacter = ',';
        var tagPart = string.Empty;
        var fieldPart = string.Empty;
        var position = GetTextPart(line, 0, out string? measurementName, new char[] { ' ', ',' });
        result.Device = LineProtocolSyntax.UnescapeName(measurementName);

        if (line[position++] == tagFollowCharacter)
        {
            position = GetTextPart(line, position, out tagPart, blankDelimiter);
            position++;
            tagPart = LineProtocolSyntax.UnescapeName(tagPart);
            var tagPairs = tagPart.Split(',').Select(pair => pair.Split('='));
            foreach (var tagPair in tagPairs)
            {
                if (tagPair.Length == 2)
                {
                    if (tagPair[0] == "Quality")
                    {
                        if (Enum.TryParse<QualityInfos>(tagPair[1], out var qualityInfo))
                        {
                            result.Quality = qualityInfo;
                        }
                    }
                    else
                    {
                        result.Tags.Add(tagPair[0], tagPair[1]);
                    }
                }
            }
        }
        position = GetTextPart(line, position, out fieldPart, blankDelimiter);
        fieldPart = LineProtocolSyntax.UnescapeName(fieldPart);
        var fieldPairs = fieldPart.Split(',').Select(pair => pair.Split('='));
        foreach (var fieldPair in fieldPairs)
        {
            if (fieldPair.Length == 2)
            {
                var value = ConvertValue(fieldPair[1]);
                result.Values.Add(new MeasuredValue { Name = fieldPair[0], Value = value });
            }
        }
        var epochText = line.Substring(position + 1).TrimEnd();
        if (long.TryParse(epochText, out var epockTicks))
        {
            result.Ticks = epockTicks.FromEpoch().ToLocalTime().Ticks;
        }
        return result;
    }

    public Dictionary<string, object> ToDictionary(List<string> tags, List<string> values)
    {
        var result = new Dictionary<string, object>();
        if (tags != null)
        {
            foreach (var key in tags)
            {
                if (Tags.ContainsKey(key))
                {
                    var measureValue = new MeasuredValue { Name = key, Value = Tags[key] };
                    result.Add(key, measureValue);
                }
            }
        }
        if (values != null)
        {
            foreach (var key in values)
            {
                var value = Values.FirstOrDefault(v => v.Name == key);
                if (value != null)
                {
                    result.Add(key, value);
                }
            }
        }
        return result;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(";Device:").Append(Device);
        sb.Append(";TimeStamp:").Append(GetTimeStamp());
        return sb.ToString();
    }

    private static object? ConvertValue(string valueString)
    {
        if (string.IsNullOrEmpty(valueString))
            return null;
        if (valueString.StartsWith("\"") || valueString.StartsWith("'"))
        {
            // string
            if (valueString.EndsWith("\"") || valueString.EndsWith("'"))
            {
                return valueString.Substring(1, valueString.Length - 2);
            }
            throw new ArgumentException($"cannot convert argument to string: {valueString}");
        }
        if (valueString.EndsWith("i"))
        {
            // integer
            if (long.TryParse(valueString.Substring(0, valueString.Length - 1), out var intResult))
            {
                return intResult;
            }
            throw new ArgumentException($"cannot convert argument to long: {valueString}");
        }
        if (char.IsLetter(valueString[0]))
        {
            // boolean
            var lowerCase = valueString.ToLower();
            if (lowerCase == "t" || lowerCase == "true")
            {
                return true;
            }
            if (lowerCase == "f" || lowerCase == "false")
            {
                return false;
            }
            throw new ArgumentException($"cannot convert argument to boolean: {valueString}");
        }
        if (double.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleResult))
        {
            return doubleResult;
        }
        throw new ArgumentException($"cannot convert argument to double: {valueString}");
    }

    private static int GetTextPart(string line, int startPos, out string? textPart, char[] delimiters)
    {
        if (delimiters == null || delimiters.Length <= 0)
            throw new ArgumentNullException($"{nameof(delimiters)} is null or empty.");
        if (string.IsNullOrEmpty(line))
            throw new ArgumentNullException($"{nameof(line)} is null");
        for (var p = startPos; p < line.Length; p++)
        {
            var c = line[p];
            if (c == '\"' || c == '\'')
            {
                // string
                var stringEndChar = c;
                p++;
                while (p < line.Length)
                {
                    c = line[p++];
                    if (c == '\\')
                        continue;
                    if (c == stringEndChar)
                        break;
                }
            }
            if (c == '\\')
            {
                // escape character; ignore next character
                p++;
                continue;
            }
            for (var d = 0; d < delimiters.Length; d++)
            {
                if (c == delimiters[d])
                {
                    textPart = line.Substring(startPos, p - startPos);
                    return p;
                }
            }
        }
        textPart = null;
        return line.Length;
    }
}