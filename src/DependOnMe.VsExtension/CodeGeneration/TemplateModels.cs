using CodeJam;
using System.Collections.Generic;

namespace DependOnMe.VsExtension.CodeGeneration
{
    public sealed class ModuleRegistration
    {
        public readonly string Name;

        public ModuleRegistration(string name)
        {
            Code.NotNullNorWhiteSpace(name, nameof(name));

            Name = name;
        }
    }

    public sealed class DependencyRegistration
    {
        public readonly string Dependency;
        public readonly string Implementation;

        public DependencyRegistration(string dependency, string implementation)
        {
            Code.NotNullNorWhiteSpace(dependency, nameof(dependency));
            Code.NotNullNorWhiteSpace(implementation, nameof(implementation));

            Dependency = dependency;
            Implementation = implementation;
        }
    }

    public sealed class TestDefinition
    {
        public readonly string Name;
        public readonly bool BoolFlag1Value;
        public readonly bool BoolFlag2Value;
        public readonly IReadOnlyCollection<DependencyRegistration> DependencyRegistrations;
        public readonly IReadOnlyCollection<ModuleRegistration> ModuleRegistrations;

        public TestDefinition(
            string name, 
            bool boolFlag1Value,
            bool boolFlag2Value,
            IReadOnlyCollection<DependencyRegistration> dependencyRegistrations, 
            IReadOnlyCollection<ModuleRegistration> moduleRegistrations)
        {
            Code.NotNullNorWhiteSpace(name, nameof(name));
            Code.NotNull(dependencyRegistrations, nameof(dependencyRegistrations));
            Code.NotNull(moduleRegistrations, nameof(moduleRegistrations));

            Name = name;
            BoolFlag1Value = boolFlag1Value;
            BoolFlag2Value = boolFlag2Value;
            DependencyRegistrations = dependencyRegistrations;
            ModuleRegistrations = moduleRegistrations;
        }
    }

    public sealed class TestTemplateModel
    {
        public readonly string NamespaceDef;
        public readonly IReadOnlyCollection<TestDefinition> Definitions;

        public TestTemplateModel(string namespaceDef, IReadOnlyCollection<TestDefinition> definitions)
        {
            Code.NotNullNorWhiteSpace(namespaceDef, nameof(namespaceDef));
            Code.NotNull(definitions, nameof(definitions));

            NamespaceDef = namespaceDef;
            Definitions = definitions;
        }
    }

    public sealed class ModuleDefinition
    {
        public readonly string Name;
        public readonly IReadOnlyCollection<DependencyRegistration> DependencyRegistrations;
        public readonly IReadOnlyCollection<ModuleRegistration> ModuleRegistrations;

        public ModuleDefinition(
            string name,
            IReadOnlyCollection<DependencyRegistration> dependencyRegistrations,
            IReadOnlyCollection<ModuleRegistration> moduleRegistrations)
        {
            Code.NotNullNorWhiteSpace(name, nameof(name));
            Code.NotNull(dependencyRegistrations, nameof(dependencyRegistrations));
            Code.NotNull(moduleRegistrations, nameof(moduleRegistrations));

            Name = name;
            DependencyRegistrations = dependencyRegistrations;
            ModuleRegistrations = moduleRegistrations;
        }
    }

    public sealed class ModuleTemplateModel
    {
        public readonly string NamespaceDef;
        public readonly IReadOnlyCollection<ModuleDefinition> Definitions;

        public ModuleTemplateModel(string namespaceDef, IReadOnlyCollection<ModuleDefinition> definitions)
        {
            Code.NotNullNorWhiteSpace(namespaceDef, nameof(namespaceDef));
            Code.NotNull(definitions, nameof(definitions));

            NamespaceDef = namespaceDef;
            Definitions = definitions;
        }
    }
}
