# 🧪 Test Commands Reference

## Quick Commands for Running Tests with Detailed Logging

### 1. **WebAPI Tests with Detailed Logs**
```bash
dotnet test WebAPI.Tests --logger "console;verbosity=detailed" --verbosity normal
```

### 2. **WebApp Tests with Detailed Logs**
```bash
dotnet test WebApp.Tests --logger "console;verbosity=detailed" --verbosity normal
```

### 3. **All Tests with Detailed Logs**
```bash
dotnet test --logger "console;verbosity=detailed" --verbosity normal
```

### 4. **Tests with Live Output (Real-time)**
```bash
dotnet test --logger "console;verbosity=detailed" --verbosity normal --results-directory ./TestResults
```

### 5. **Tests with Coverage**
```bash
dotnet test --collect:"XPlat Code Coverage" --logger "console;verbosity=detailed"
```

### 6. **Run Single Test Class**
```bash
dotnet test --filter "FullyQualifiedName~AuthControllerTests" --logger "console;verbosity=detailed"
```

### 7. **Run Single Test Method**
```bash
dotnet test --filter "FullyQualifiedName~Login_WithValidCredentials_ReturnsOkWithToken" --logger "console;verbosity=detailed"
```

### 8. **Run Tests with Specific Pattern**
```bash
dotnet test --filter "TestCategory=Auth" --logger "console;verbosity=detailed"
```

## 🚀 Custom Test Runner Script

You can also use our custom test runner:
```bash
./run-tests.sh
```

## 📊 Understanding the Log Output

When you run tests with detailed logging, you'll see:

- **🧪 STARTING TEST**: Test initialization
- **📋 STEP**: Individual test steps
- **📊 Data**: JSON formatted test data
- **✅ RESULT**: Test completion status
- **⏰ Timestamps**: When tests start/complete
- **ℹ️ INFO**: General information
- **🚨 ERROR**: Error messages (if any)

## 🎯 Pro Tips

1. **Filter specific tests**: Use `--filter` to run only specific tests
2. **Save results**: Use `--results-directory` to save detailed reports
3. **Parallel execution**: Add `--parallel` for faster execution
4. **Watch mode**: Use `dotnet watch test` for continuous testing
5. **Debug mode**: Use `--debug` to attach debugger

## 🔧 CI/CD Integration

The tests are configured for GitHub Actions with the workflow file:
`.github/workflows/tests.yml`

This automatically runs all tests with detailed logging on:
- Push to main/master/develop branches
- Pull requests
- Manual dispatch