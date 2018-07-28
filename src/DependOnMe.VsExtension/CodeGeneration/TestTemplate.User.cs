using CodeJam;

namespace DependOnMe.VsExtension.CodeGeneration
{
    public partial class TestTemplate
    {
        public readonly TestTemplateModel Model;

        public TestTemplate(TestTemplateModel model)
        {
            Code.NotNull(model, nameof(model));

            Model = model;
        }
    }
}
