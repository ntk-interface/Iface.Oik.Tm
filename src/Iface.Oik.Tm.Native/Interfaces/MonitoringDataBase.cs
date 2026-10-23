using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class MonitoringDataBase
{
    internal static unsafe T Create<T>(nint stringsPtr, Encoding? encoding = null)
        where T : MonitoringDataBase, new()
    {
        var data = new T();

        var dto = new InitializeDto();

        encoding ??= Encoding.UTF8;

        var p = (byte*)stringsPtr.ToPointer();
        var length = 0;

        while (true)
        {
            while (p[length] != 0)
            {
                length++;
            }

            var line = new Span<byte>(p, length);
            var dot = line.IndexOf((byte)'.');
            var eq = line.IndexOf((byte)'=');

            if (eq < 0 || dot < 0)
            {
                break;
            }

            var key = encoding.GetString(line[..dot]);
            var value = encoding.GetString(line[(eq + 1)..]);

            dto.TagsDictionaty.Add(key, value);

            switch (key)
            {
                case "SoftwareVersion":
                    dto.SoftwareVersion = value;
                    break;
                case "IfpcKey":
                    dto.LicenseId = value;
                    break;
                case "BuildDate":
                    dto.BuildDate = value;
                    break;
                case "IfpcRunning":
                    dto.ServerIsRunning = ParsePlusMinusBool(value);
                    break;
                case "LocalTime":
                {
                    dto.LocalTime = NativeDateUtil.GetDateTimeFromExtendedReversedTmString(value);
                    break;
                }
                case "ServerTime":
                {
                    dto.ServerTime = NativeDateUtil.GetDateTimeFromExtendedReversedTmString(value);
                    break;
                }
                case "ElixTime":
                {
                    dto.ElixTime = NativeDateUtil.GetDateTimeFromExtendedReversedTmString(value);
                    break;
                }
                case "SysUptime" when TryParseTimeSpanFromNative(value, out var val):
                {
                    dto.SysUpTime = val;
                    break;
                }
                case "TMSUptime" when TryParseTimeSpanFromNative(value, out var val):
                {
                    dto.TmsUpTime = val;
                    break;
                }
                case "PhysMem" when TryParseDecimal(value, out var val):
                {
                    dto.MemoryTotalMb = val;
                    break;
                }
                case "PhysMemUsed" when TryParseDecimal(value, out var val):
                {
                    dto.MemoryUsedMb = val;
                    break;
                }
                case "IfpcMem" when TryParseDecimal(value, out var val):
                {
                    dto.MemoryServerUsedMb = val;
                    break;
                }
                case "DiskTotal" when TryParseDecimal(value, out var val):
                {
                    dto.DiskSpaceTotalMb = val * 1024;
                    break;
                }
                case "DiskFree" when TryParseDecimal(value, out var val):
                {
                    dto.DiskSpaceFreeMb = val * 1024;
                    break;
                }
                case "DiskHealth" when int.TryParse(value, out var val):
                {
                    dto.DiskHealthPercents = val;
                    break;
                }
                case "IfpcLogErrors" when int.TryParse(value, out var val):
                {
                    dto.LogErrorsCount = val;
                    break;
                }
                case "TMSMaxUsers" when int.TryParse(value, out var val):
                {
                    dto.TmsUsersMax = val;
                    break;
                }
                case "TMSUsersCnt" when int.TryParse(value, out var val):
                {
                    dto.TmsUsersUsed = val;
                    break;
                }
                case "CpuLoad" when TryParseDecimal(value, out var val):
                {
                    dto.CpuLoadPercent = val;
                    break;
                }
                case "StatusPS" when TryParseDecimal(value, out var val):
                {
                    dto.StatusPerSecond = val;
                    break;
                }
                case "AnalogPS" when TryParseDecimal(value, out var val):
                {
                    dto.AnalogPerSecond = val;
                    break;
                }
                case "TeleparmsPS" when TryParseDecimal(value, out var val):
                {
                    dto.TeleparamsPerSecond = val;
                    break;
                }
                case "TMS_TPMax" when int.TryParse(value, out var val):
                {
                    dto.TmsTeleparamsMax = val;
                    break;
                }
                case "TMS_TPLeft" when int.TryParse(value, out var val):
                {
                    dto.TmsTeleparamsLeft = val;
                    break;
                }
            }

            length++;

            if (p[length] == 0)
            {
                break;
            }

            p += length;
            length = 0;
        }

        data.Initialize(dto);

        return data;
    }

    internal static bool? ParsePlusMinusBool(string str)
    {
        return str switch
        {
            "+" => true,
            "-" => false,
            _ => null,
        };
    }

    internal static bool TryParseTimeSpanFromNative(string str, out TimeSpan result)
    {
        var formats = new[]
        {
            @"d' д, 'h' ч, 'm' м, 's' с'",
            @"h' ч, 'm' м, 's' с'",
            @"m' м, 's' с'",
            @"s' с'",
        };

        return TimeSpan.TryParseExact(str, formats, CultureInfo.InvariantCulture, out result);
    }

    internal static bool TryParseDecimal(string str, out decimal value)
    {
        return decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    protected abstract void Initialize(InitializeDto dto);

    protected record InitializeDto
    {
        public Dictionary<string, string> TagsDictionaty { get; set; } = new();

        public string SoftwareVersion { get; set; } = string.Empty;
        public string BuildDate { get; set; } = string.Empty;
        public bool? ServerIsRunning { get; set; }
        public string LicenseId { get; set; } = string.Empty;
        public DateTime? LocalTime { get; set; }
        public DateTime? ServerTime { get; set; }
        public DateTime? ElixTime { get; set; }
        public TimeSpan? SysUpTime { get; set; }
        public TimeSpan? TmsUpTime { get; set; }
        public decimal? MemoryTotalMb { get; set; }
        public decimal? MemoryUsedMb { get; set; }
        public decimal? MemoryServerUsedMb { get; set; }
        public int? LogErrorsCount { get; set; }
        public decimal? DiskSpaceTotalMb { get; set; }
        public decimal? DiskSpaceFreeMb { get; set; }
        public int? DiskHealthPercents { get; set; }

        public int? TmsUsersMax { get; set; }
        public int? TmsUsersUsed { get; set; }
        public decimal? CpuLoadPercent { get; set; }
        public decimal? StatusPerSecond { get; set; }
        public decimal? AnalogPerSecond { get; set; }
        public decimal? TeleparamsPerSecond { get; set; }
        public int? TmsTeleparamsLeft { get; set; }
        public int? TmsTeleparamsMax { get; set; }
    }
}
