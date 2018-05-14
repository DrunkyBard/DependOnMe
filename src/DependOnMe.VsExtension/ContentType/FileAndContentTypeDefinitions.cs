using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.ContentType
{
    internal static class FileAndContentTypeDefinitions
	{
		[Export]
		[Name("hid")]
		[BaseDefinition("text")]
		internal static ContentTypeDefinition HidingContentTypeDefinition;

		[Export]
		[FileExtension(".hid")]
		[ContentType("hid")]
		internal static FileExtensionToContentTypeDefinition HiddenFileExtensionDefinition;
	}
}
