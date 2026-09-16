# NServiceBus Logging Scope Demo

This sample shows how `NServiceBus.Logging.ILog` behaves outside and inside an
endpoint logging slot. It uses only supported NServiceBus 10 APIs.

Run the out-of-slot case:

```powershell
dotnet run -- out-of-slot
Get-ChildItem .\bin\Debug\net10.0\nsb_log_*.txt
```

The two messages are written by NServiceBus's fallback logger, and an
`nsb_log_*.txt` file is created.

Then run the scoped case:

```powershell
dotnet run -- scoped
Get-ChildItem .\bin\Debug\net10.0\nsb_log_*.txt
```

The same messages are displayed by Microsoft console logging, and no fallback
file is created.

Each mode deletes stale `nsb_log_*.txt` files before starting. This matters
because `dotnet clean` does not remove files created dynamically at runtime.

`LogManager.GetLogger` is not deprecated; it represents the static logger used
by NServiceBus components. The example does not call the deprecated
`LogManager.Use` or `LogManager.UseFactory` configuration APIs.

The example uses one endpoint. A host containing multiple endpoints must select
the appropriate `EndpointLoggingScope`.
