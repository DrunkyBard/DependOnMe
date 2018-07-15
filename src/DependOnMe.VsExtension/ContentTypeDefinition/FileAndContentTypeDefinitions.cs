using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

#pragma warning disable CS0649

namespace DependOnMe.VsExtension.ContentTypeDefinition
{
    internal static class FileAndContentTypeDefinitions
	{
		[Export]
		[Name(ContentType.Test)]
		[BaseDefinition("text")]
		internal static Microsoft.VisualStudio.Utilities.ContentTypeDefinition DependencyTestContentTypeDefinition;

		[Export]
		[FileExtension(ContentType.DotTest)]
		[ContentType(ContentType.Test)]
        internal static FileExtensionToContentTypeDefinition DependencyTestFileExtensionDefinition;

	    [Export]
		[Name(ContentType.Module)]
		[BaseDefinition("text")]
		internal static Microsoft.VisualStudio.Utilities.ContentTypeDefinition DependencyModuleContentTypeDefinition;

		[Export]
		[FileExtension(ContentType.DotModule)]
		[ContentType(ContentType.Module)]
        internal static FileExtensionToContentTypeDefinition DependencyModuleFileExtensionDefinition;
	}
}
