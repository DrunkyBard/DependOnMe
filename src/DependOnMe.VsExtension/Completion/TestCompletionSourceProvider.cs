﻿using DependOnMe.VsExtension.ContentTypeDefinition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.Completion
{
    [Export(typeof(ICompletionSourceProvider))]
	[ContentType(ContentType.Test)]
	[Name("token completion")]
	public sealed class TestCompletionSourceProvider : ICompletionSourceProvider
	{
		[Import]
		public ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        public SVsServiceProvider ServiceProvider;

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer) => new TestCompletionSource(this, textBuffer, ServiceProvider);
	}
}
