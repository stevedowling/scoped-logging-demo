# NServiceBus Logging Scope Demo

This sample shows how `NServiceBus.Logging.ILog` behaves outside and inside an
endpoint logging slot. It uses only supported NServiceBus 10 APIs.

Run the out-of-slot case:

```powershell
dotnet run -- out-of-slot
```

The two messages are written by NServiceBus's fallback logger, and an
`nsb_log_*.txt` file is created.

Then run the scoped case:

```powershell
dotnet run -- scoped
```

The same messages are displayed by Microsoft console logging, and no fallback
file is created.
