# statsd-helper-webapi

Filter attributes to allow easy metrics gathering based on Response Codes.

See https://github.com/paybyphone/statsd-helper configuring the statsd-helper itself.

Along with the prefix generated by the statsd-helper, the metric will append the action name and status code to the metric.

For example:
```
com.example.servername.api.validate.404
```

Usage:

Add the filters to your WebApiConfiguration.

```
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            ...
            config.ConfigureInstrumentFilters();
            ...
        }
        
        private static void ConfigureInstrumentFilters(this HttpConfiguration config)
        {
            config.Filters.Add(new InstrumentStatusCodeFilterAttribute());
            config.Filters.Add(new InstrumentStatusCodeExceptionFilterAttribute());
        }
    }
```
