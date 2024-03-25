using System.Text.RegularExpressions;

namespace HyperTensionBot.Server.Bot.Extensions {
    public static class RegexExtensions {
        public static int GetIntMatch(this Match match, string groupName) {
            return match.GetOptionalIntMatch(groupName) ?? throw new ArgumentException($"Group {groupName} not matched or not convertible to integer");
        }

        public static int? GetOptionalIntMatch(this Match match, string groupName) {
            var g = match.Groups[groupName] ?? throw new ArgumentException($"Group {groupName} not found");
            if (!g.Success) {
                return null;
            }

            if (!int.TryParse(g.ValueSpan, out var result)) {
                return null;
            }

            return result;
        }

        public static double?[] ExtractMeasurement(string message) {
            var match = Regex.Match(message, @"(?<v1>\d{1,3})\D+(?<v2>\d{1,3})\D+(?<v3>\d{1,3})");

            if (!match.Success) {
                throw new ArgumentException("Il messaggio non contiene numeri decimali.");
            }
            double? sistolyc = (double.Parse(match.Groups["v1"].Value) != 0) ? Math.MaxMagnitude(double.Parse(match.Groups["v1"].Value), double.Parse(match.Groups["v2"].Value)) : null;
            double? diastolic = (double.Parse(match.Groups["v1"].Value) != 0) ? Math.MinMagnitude(double.Parse(match.Groups["v1"].Value), double.Parse(match.Groups["v2"].Value)) : null;
            double? frequence = (double.Parse(match.Groups["v3"].Value) != 0) ? double.Parse(match.Groups["v3"].Value) : null;
            return new[] { sistolyc, diastolic, frequence};
        }

        public static string[] ExtractParameters(string message) {
            var match = Regex.Match(message, @"(?<v1>\w+)\s*(?<v2>[\+\-]?\d+)\s*(?<v3>\w+)");
            if (!match.Success) {
                throw new ArgumentException("L'output non contiene tre parametri.");
            }
            return new[] { match.Groups["v1"].Value, match.Groups["v2"].Value, match.Groups["v3"].Value };
        }
    }
}
