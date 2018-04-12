using TDCR.Daemon.Wire;

namespace TDCR.Daemon
{
    public static class ApiExtensions
    {
        public static string GetVer(this SemVer semver)
        {
            return $"{semver.Major}.{semver.Minor}.{semver.Patch}";
        }
    }
}
