using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace DependOnMe.VsExtension.Coloring
{
    internal static class TermDefinitions
    {
        [Export]
        [Name(Classification.Keyword)]
        [BaseDefinition(Classification.Keyword)]
        internal static ClassificationTypeDefinition KeywordDefinition;

        [Export]
        [Name(Classification.Error)]
        [BaseDefinition(Classification.Error)]
        internal static ClassificationTypeDefinition ErrorDefinition;

        [Export]
        [Name(Classification.Default)]
        [BaseDefinition(Classification.Default)]
        internal static ClassificationTypeDefinition DefaultDefinition;

        [Export]
        [Name(Classification.Dependency)]
        [BaseDefinition(Classification.Dependency)]
        internal static ClassificationTypeDefinition DependencyDefinition;

        [Export]
        [Name(Classification.Implementation)]
        [BaseDefinition(Classification.Implementation)]
        internal static ClassificationTypeDefinition ImplementationDefinition;

        [Export]
        [Name(Classification.Sign)]
        [BaseDefinition(Classification.Sign)]
        internal static ClassificationTypeDefinition SignDefinition;


        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = Classification.Keyword)]
        [Name(Classification.Keyword)]
        internal sealed class KeywordFormat : ClassificationFormatDefinition
        {
            public KeywordFormat()
            {
                ForegroundColor = Colors.DodgerBlue;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = Classification.Error)]
        [Name(Classification.Error)]
        internal sealed class ErrorFormat : ClassificationFormatDefinition
        {
            public ErrorFormat()
            {
                ForegroundColor = Colors.Red;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = Classification.Default)]
        [Name(Classification.Default)]
        internal sealed class DefaultFormat : ClassificationFormatDefinition
        {
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = Classification.Dependency)]
        [Name(Classification.Dependency)]
        internal sealed class DependencyFormat : ClassificationFormatDefinition
        {
            public DependencyFormat()
            {
                ForegroundColor = Colors.MediumSeaGreen;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = Classification.Implementation)]
        [Name(Classification.Implementation)]
        internal sealed class ImplementationFormat : ClassificationFormatDefinition
        {
            public ImplementationFormat()
            {
                ForegroundColor = Colors.MediumSpringGreen;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = Classification.Sign)]
        [Name(Classification.Sign)]
        internal sealed class SignFormat : ClassificationFormatDefinition
        {
            public SignFormat()
            {
                ForegroundColor = Colors.MediumSpringGreen;
            }
        }
    }
}
