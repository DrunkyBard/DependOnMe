using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

#pragma warning disable CS0649

namespace DependOnMe.VsExtension.ContentType
{
    internal static class FileAndContentTypeDefinitions
	{
		[Export]
		[Name("drt")]
		[BaseDefinition("text")]
		internal static ContentTypeDefinition DependencyTestContentTypeDefinition;

		[Export]
		[FileExtension(".drt")]
		[ContentType("drt")]
        internal static FileExtensionToContentTypeDefinition DependencyTestFileExtensionDefinition;

	    [Export]
		[Name("drm")]
		[BaseDefinition("text")]
		internal static ContentTypeDefinition DependencyModuleContentTypeDefinition;

		[Export]
		[FileExtension(".drm")]
		[ContentType("drm")]
        internal static FileExtensionToContentTypeDefinition DependencyModuleFileExtensionDefinition;
	}
}
