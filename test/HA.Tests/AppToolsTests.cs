using HA.Tests.Utils;
using Microsoft.Extensions.Logging;

namespace HA.Tests;

public class AppToolsTests
{
    private ILoggerFactory _loggerFactory;

    [SetUp]
    public void Setup()
    {
        _loggerFactory = LoggerFactory.Create(builder =>
            builder.AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("ha", LogLevel.Debug));
    }

    [Test]
    public void check_environment_variables_are_assigned_to_properties_with_EnvParameterAttribute()
    {
        Environment.SetEnvironmentVariable("ENV_STRING", "ABC", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("ENV_INT", "1234", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("ENV_BOOL", "true", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("ENV_DOUBLE", "1.234", EnvironmentVariableTarget.Process);

        var appSettings = new AppSettings(_loggerFactory.CreateLogger<AppSettings>());
        Assert.That(appSettings.EnvString, Is.EqualTo("ABC"));
        Assert.That(appSettings.EnvInt32, Is.EqualTo(1234));
        Assert.That(appSettings.EnvBool, Is.EqualTo(true));
        Assert.That(appSettings.EnvDouble, Is.EqualTo(1.234));
    }

    [Test]
    public void check_environment_variables_are_partially_assigned_to_properties_with_EnvParameterAttribute()
    {
        var appSettings = new AppSettings(_loggerFactory.CreateLogger<AppSettings>());

        appSettings.ReadConfigFile("appsettings.json");

        Assert.That(appSettings.EnvString, Is.EqualTo("ABC"));

        appSettings.ShowEnvironmentVariables();
        //Assert.That(appSettings.EnvInt32, Is.EqualTo(1234));
        //Assert.That(appSettings.EnvBool, Is.EqualTo(true));
        //Assert.That(appSettings.EnvDouble, Is.EqualTo(1.234));
    }
}