using TDCR.Daemon.Wire;

namespace TDCR.Daemon
{
    public static class Constants
    {
        public static class Version
        {
            public const uint Major = 1;
            public const uint Minor = 0;
            public const uint Patch = 0;
            public static readonly SemVer SemVer = new SemVer { Major = Major, Minor = Minor, Patch = Patch };
        }
    }
}
