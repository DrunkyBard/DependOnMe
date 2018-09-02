using CodeJam;
using Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Error = Errors.FlattenError;

namespace DependOnMe.VsExtension.CodeGeneration
{
    public struct GenerationUnit
    {
        private static readonly IReadOnlyCollection<Error> EmptyErrors = Enumerable.Empty<Error>().AsReadOnly();
        private readonly byte[] _src;

        public byte[] Src
        {
            get
            {
                if (HasErrors())
                {
                    throw new InvalidOperationException($"Source code '{OriginSrcPath}' contains errors. Check with {nameof(HasErrors)} method first");
                }

                return _src;
            }
        }

        public readonly string OriginSrcPath;
        public readonly IReadOnlyCollection<Error> Errors;

        private GenerationUnit(string originSrcPath, byte[] src, IReadOnlyCollection<Error> errors)
        {
            Code.NotNull(originSrcPath, nameof(originSrcPath));
            //Code.AssertArgument(errors.Count > 0 && src == null, nameof(src), $"Or {nameof(src)} argument should be null, or {nameof(errors)} argument should be not empty");

            _src = src;
            OriginSrcPath = originSrcPath;
            Errors = errors;
        }

        public bool HasErrors() => Errors.Count > 0;

        public static GenerationUnit Complete(byte[] src, string originPath) => new GenerationUnit(originPath, src, EmptyErrors);

        public static GenerationUnit Error(IReadOnlyCollection<Error> errors, string originPath) => new GenerationUnit(originPath, null, errors);
    }

    internal static class CodeGenerator
    {
        public static GenerationUnit GenerateDrt(string src, string defaultProjNamespace, string filePath)
        {
            Code.NotNull(src, nameof(src));
            Code.NotNullNorWhiteSpace(filePath, nameof(filePath));

            RefTable.Instance.TryRemoveTestRefs(filePath);
            var cUnit   = Compiler.Instance.CompileTestOnFly(src, filePath);
            var cErrors = cUnit.Errors.Select(err => err.Flatten()).AsReadOnly();

            if (cErrors.Count > 0)
            {
                return GenerationUnit.Error(cErrors, filePath);
            }

            var testDefs = cUnit
                .OnlyValidTests()
                .ValidTests
                .Select(x => new TestDefinition(
                    x.Name,
                    x.BoolFlag1Values.Length > 0 && x.BoolFlag1Values.First().Value,
                    x.BoolFlag2Values.Length > 0 && x.BoolFlag2Values.First().Value,
                    x.ClassRegistrations.Select(y => new DependencyRegistration(y.Dependency, y.Implementation)).AsReadOnly(),
                    x.RegisteredModules.Select(y => new ModuleRegistration(y.Name)).AsReadOnly()))
                .AsReadOnly();
            var templateModel = new TestTemplateModel(defaultProjNamespace, testDefs);
            var template      = new TestTemplate(templateModel);
            var csSrc         = template.TransformText();

            return GenerationUnit.Complete(Encoding.UTF8.GetBytes(csSrc), filePath);
        }

        public static GenerationUnit GenerateDrm(string src, string defaultProjNamespace, string filePath)
        {
            Code.NotNull(src, nameof(src));
            Code.NotNullNorWhiteSpace(filePath, nameof(filePath));

            RefTable.Instance.TryRemoveDeclarations(filePath);
            var cUnit = Compiler.Instance.CompileModuleOnFly(src, filePath);
            var cErrors = cUnit.Errors.Select(err => err.Flatten()).AsReadOnly();

            if (cErrors.Count > 0)
            {
                return GenerationUnit.Error(cErrors, filePath);
            }

            var testDefs = cUnit
                .OnlyValidModules()
                .ValidModules
                .Select(x => new ModuleDefinition(
                    x.Name,
                    x.ClassRegistrations.Select(y => new DependencyRegistration(y.Dependency, y.Implementation)).AsReadOnly(),
                    x.ModuleRegistrations.Select(y => new ModuleRegistration(y.Name)).AsReadOnly()))
                .AsReadOnly();
            var templateModel = new ModuleTemplateModel(defaultProjNamespace, testDefs);
            var template      = new ModuleTemplate(templateModel);
            var csSrc         = template.TransformText();

            return GenerationUnit.Complete(Encoding.UTF8.GetBytes(csSrc), filePath);
        }
    }
}
