namespace DependOnMe.VsExtension.ContentTypeDefinition
{
    internal sealed class ContentType
    {
        public const string Module = "drm";
        public const string Test   = "drt";

        public const string DotModule = ".drm";
        public const string DotTest   = ".drt";
    }

    internal enum DslType
    {
        Test,
        Module
    }
}
