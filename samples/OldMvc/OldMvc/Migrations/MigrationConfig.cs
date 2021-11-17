using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.AspNet.Migrations
{
    internal class MigrationConfig
    {
        public const string FileName = "migration.json";

        [JsonConstructor]
        public MigrationConfig(
            Dictionary<string, MigrationRouteOptions> routes = null,
            bool @lock = false)
        {
            _routes = routes ?? new Dictionary<string, MigrationRouteOptions>(StringComparer.OrdinalIgnoreCase);
            Lock = @lock;
        }

        // TODO build prefix tree or something else to avoid going through all routes on every request if number of routes is large?
        private Dictionary<string, MigrationRouteOptions> _routes;
        public IEnumerable<KeyValuePair<string, MigrationRouteOptions>> Routes => _routes;
        public bool Lock { get; }

        public MigrationConfig AddRoute(string url, MigrationRouteOptions options = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                return this;
            }

            _routes[url] = options ?? new MigrationRouteOptions();

            return this;
        }
    }

    internal class MigrationRouteOptions
    {
        [JsonConstructor]
        public MigrationRouteOptions(
            string match = null,
            string mapTo = null,
            bool redirect = false,
            bool @static = false,
            bool @lock = false)
        {
            Match = match is null || !Enum.TryParse<MigrationRouteMatch>(match, out var matchValue)
                ? MigrationRouteMatch.Default
                : matchValue;
            Redirect = redirect;
            Static = @static;
            MapTo = mapTo;
            Lock = @lock;
        }

        public MigrationRouteMatch Match { get; }
        public string MapTo { get; }
        public bool Static { get; }
        public bool Redirect { get; }
        public bool Lock { get; }
    }

    internal enum MigrationRouteMatch
    {
        Default,
        StartsWith,
        EndsWith,
        Regex
    }
}