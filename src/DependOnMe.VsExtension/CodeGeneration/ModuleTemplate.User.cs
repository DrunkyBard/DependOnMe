using CodeJam;

namespace DependOnMe.VsExtension.CodeGeneration
{
    public partial class ModuleTemplate
    {
        public readonly ModuleTemplateModel Model;
        public ModuleTemplate(ModuleTemplateModel model)
        {
            Code.NotNull(model, nameof(model));

            Model = model;
        }
    }
}

