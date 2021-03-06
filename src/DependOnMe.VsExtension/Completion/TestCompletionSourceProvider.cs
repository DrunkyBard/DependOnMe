﻿using DependOnMe.VsExtension.ContentTypeDefinition;
using Microsoft.FSharp.Text.Lexing;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.Completion
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType(ContentType.Test)]
    [Name("Test token completion")]
    public sealed class TestCompletionSourceProvider : BaseCompletionSourceProvider
    {
        protected override IReadOnlyCollection<string> GetSuggestions(string fileName, string src, Position position)
            => TestDslCompletion.suggestFrom(fileName, src, position);
    }
}
