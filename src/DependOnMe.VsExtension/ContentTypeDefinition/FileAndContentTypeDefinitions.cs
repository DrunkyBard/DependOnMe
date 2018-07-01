using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

#pragma warning disable CS0649

namespace DependOnMe.VsExtension.ContentTypeDefinition
{
    internal static class FileAndContentTypeDefinitions
	{
		[Export]
		[Name(ContentTypeDefinition.ContentType.Test)]
		[BaseDefinition("text")]
		internal static Microsoft.VisualStudio.Utilities.ContentTypeDefinition DependencyTestContentTypeDefinition;

		[Export]
		[FileExtension(ContentTypeDefinition.ContentType.DotTest)]
		[ContentType(ContentTypeDefinition.ContentType.Test)]
        internal static FileExtensionToContentTypeDefinition DependencyTestFileExtensionDefinition;

	    [Export]
		[Name(ContentTypeDefinition.ContentType.Module)]
		[BaseDefinition("text")]
		internal static Microsoft.VisualStudio.Utilities.ContentTypeDefinition DependencyModuleContentTypeDefinition;

		[Export]
		[FileExtension(ContentTypeDefinition.ContentType.DotModule)]
		[ContentType(ContentTypeDefinition.ContentType.Module)]
        internal static FileExtensionToContentTypeDefinition DependencyModuleFileExtensionDefinition;
	}
}
