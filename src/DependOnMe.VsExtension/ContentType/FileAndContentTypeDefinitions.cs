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
		internal static ContentTypeDefinition HidingContentTypeDefinition;

		[Export]
		[FileExtension(".drt")]
		[ContentType("drt")]
        internal static FileExtensionToContentTypeDefinition HiddenFileExtensionDefinition;
	}
}
