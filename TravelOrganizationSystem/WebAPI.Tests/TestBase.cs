using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace WebAPI.Tests;

public abstract class TestBase
{
    protected readonly ITestOutputHelper Output;
    protected readonly ILogger Logger;

    protected TestBase(ITestOutputHelper output)
    {
        Output = output;
        Logger = new TestLogger(output);
    }

    protected void LogTestStart(string testName)
    {
        Output.WriteLine("=".PadRight(80, '='));
        Output.WriteLine($"🧪 STARTING TEST: {testName}");
        Output.WriteLine($"⏰ Test started at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
        Output.WriteLine("=".PadRight(80, '='));
    }

    protected void LogTestStep(string step, object? data = null)
    {
        Output.WriteLine($"📋 STEP: {step}");
        if (data != null)
        {
            Output.WriteLine($"   📊 Data: {System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}");
        }
    }

    protected void LogTestResult(string result, bool success = true)
    {
        var emoji = success ? "✅" : "❌";
        Output.WriteLine($"{emoji} RESULT: {result}");
    }

    protected void LogTestEnd(string testName, bool success = true)
    {
        var emoji = success ? "✅" : "❌";
        Output.WriteLine("-".PadRight(80, '-'));
        Output.WriteLine($"{emoji} TEST COMPLETED: {testName}");
        Output.WriteLine($"⏰ Test completed at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
        Output.WriteLine("-".PadRight(80, '-'));
    }

    protected void LogError(string error, Exception? ex = null)
    {
        Output.WriteLine($"🚨 ERROR: {error}");
        if (ex != null)
        {
            Output.WriteLine($"   Exception: {ex.Message}");
            Output.WriteLine($"   StackTrace: {ex.StackTrace}");
        }
    }

    protected void LogWarning(string warning)
    {
        Output.WriteLine($"⚠️  WARNING: {warning}");
    }

    protected void LogInfo(string info)
    {
        Output.WriteLine($"ℹ️  INFO: {info}");
    }
}

public class TestLogger : ILogger
{
    private readonly ITestOutputHelper _output;

    public TestLogger(ITestOutputHelper output)
    {
        _output = output;
    }

    public IDisposable BeginScope<TState>(TState state) => new TestLoggerScope();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var emoji = logLevel switch
        {
            LogLevel.Critical => "🔥",
            LogLevel.Error => "🚨",
            LogLevel.Warning => "⚠️",
            LogLevel.Information => "ℹ️",
            LogLevel.Debug => "🔍",
            LogLevel.Trace => "👀",
            _ => "📝"
        };

        _output.WriteLine($"{emoji} [{logLevel}] {formatter(state, exception)}");
        if (exception != null)
        {
            _output.WriteLine($"   Exception: {exception}");
        }
    }

    private class TestLoggerScope : IDisposable
    {
        public void Dispose() { }
    }
}