# 🚀 Quick Test Commands

## The Simple Commands You Need:

### 1. **See ALL the logs** (like you just saw):
```bash
dotnet test --logger "console;verbosity=detailed" --verbosity normal
```

### 2. **Test just WebAPI with logs**:
```bash
dotnet test WebAPI.Tests --logger "console;verbosity=detailed" --verbosity normal
```

### 3. **Test just WebApp with logs**:
```bash
dotnet test WebApp.Tests --logger "console;verbosity=detailed" --verbosity normal
```

### 4. **Basic test run** (minimal output):
```bash
dotnet test
```

### 5. **Test with coverage**:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### 6. **Use our custom script** (with colors and emojis):
```bash
./run-tests.sh
```

---

## What You'll See in the Logs:

✅ **With detailed logging**, you get:
- 🧪 Test start/end markers
- 📋 Step-by-step execution
- 📊 JSON data for test inputs
- ⏰ Timestamps
- ℹ️ Info messages
- 🚨 Error details (if any)

❌ **Without detailed logging**, you get:
- Just pass/fail results
- No step details
- No data visualization

---

## Pro Tip:
The **first command** is what you want to use to see all the detailed logs we added! 🎯