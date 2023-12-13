using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HA.AppTools;

public abstract class AppSettingsBase
{
    protected readonly ILogger _logger;
    protected readonly ValueConverter valueConverter = new ValueConverter();

    protected AppSettingsBase(ILogger logger)
    {
        _logger = logger;
    }

    protected void ReadEnvironmentVariables()
    {
        var props = GetType().GetProperties();
        foreach (var prop in props)
        {
            var custAttr = prop.GetCustomAttributes(typeof(EnvParameterAttribute), true);
            if (custAttr?.Length > 0)
            {
                var parameter = custAttr.First() as EnvParameterAttribute;
                if (parameter != null)
                {
                    var name = parameter.Name;
                    var typeCode = Type.GetTypeCode(prop.PropertyType);
                    var variable = GetEnvironmentVariable(name, typeCode);
                    if (variable.available)
                    {
                        _logger.LogInformation("Use environment variable: {0} | {1} | {2}",
                            name, variable.value, typeCode.ToString());
                        prop.SetValue(this, variable.value);
                    }
                }
            }
        }
    }

    public void ShowEnvironmentVariables()
    {
        var props = GetType().GetProperties();
        foreach (var prop in props)
        {
            var custAttr = prop.GetCustomAttributes(typeof(EnvParameterAttribute), true);
            if (custAttr?.Length > 0)
            {
                var parameter = custAttr.First() as EnvParameterAttribute;
                if (parameter != null)
                {
                    var name = parameter.Name;
                    var typeCode = Type.GetTypeCode(prop.PropertyType);
                    Console.WriteLine("Environment Variable: {0} | {1}", name, typeCode.ToString());
                }
            }
        }
    }

    protected (bool available, object? value) GetEnvironmentVariable(string name, TypeCode typeCode)
    {
        var variable = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        if (variable != null)
        {
            var value = valueConverter.ConvertTo(variable, typeCode);
            if (value != null)
                return (true, value);
        }
        return (false, null);
    }

    protected void ReadAppConfigFile(IConfiguration configuration)
    {
        if (configuration == null)
        {
            _logger.LogWarning("Now configuration file provides");
            return;
        }
        var props = GetType().GetProperties();
        foreach (var prop in props)
        {
            var custAttr = prop.GetCustomAttributes(typeof(ConfigParameterAttribute), true);
            if (custAttr?.Length > 0)
            {
                var parameter = custAttr.First() as ConfigParameterAttribute;
                if (parameter != null)
                {
                    var typeCode = Type.GetTypeCode(prop.PropertyType);
                    var section = configuration.GetSection($"{parameter.Section}:{parameter.Name}");
                    if (section.Exists())
                    {
                        var value = section.Value;
                        if (value != null)
                        {
                            var propValue = valueConverter.ConvertTo(value, typeCode);
                            _logger.LogInformation("Read variable from config file : {0}:{1} | {2} | {3}",
                                parameter.Section, parameter.Name, propValue, typeCode.ToString());
                            prop.SetValue(this, propValue);
                        }
                        else
                            _logger.LogWarning("Property not found: {0}:{1}", section, parameter.Name);
                    }
                    else
                        _logger.LogWarning("Section not found: {0}:{1}", section, parameter.Name);
                }
            }
        }
    }

    protected void CheckSettings()
    {
        var props = GetType().GetProperties();
        foreach (var prop in props)
        {
            var custAttr = prop.GetCustomAttributes(typeof(ConfigParameterAttribute), true);
            if (custAttr?.Length > 0)
            {
                var parameter = custAttr.First() as ConfigParameterAttribute;
                if (parameter != null)
                {
                    var required = parameter.Required;
                    if (required && prop.GetValue(this) == null)
                    {
                        var envAttrs = prop.GetCustomAttributes(typeof(EnvParameterAttribute), true);
                        var envVariableName = (envAttrs?.Length > 0) 
                            ? ((EnvParameterAttribute)envAttrs.First()).Name
                            : string.Empty;
                        throw new ArgumentNullException($"Parameter was not provided: in file: '{parameter.Section}:{parameter.Name}' or  Enviroment: '{envVariableName}'");
                    }
                }
            }
        }
    }
}
