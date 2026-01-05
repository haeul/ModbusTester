using System;
using System.Reflection;

namespace ModbusTester.Utils
{
    public static class AppVersion
    {
        public static string Get()
        {
            var asm = Assembly.GetExecutingAssembly();

            var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            var v = string.IsNullOrWhiteSpace(info)
                ? asm.GetName().Version?.ToString() ?? "0.0.0"
                : info;

            // "1.0.0+commitsha" => "1.0.0"
            var plus = v.IndexOf('+');
            if (plus >= 0) v = v.Substring(0, plus);

            // "1.0.0.0" => "1.0.0" (원하면)
            if (Version.TryParse(v, out var ver) && ver.Build >= 0)
                v = $"{ver.Major}.{ver.Minor}.{ver.Build}";

            return v;
        }
    }
}
