using Autodesk.Forge.DesignAutomation.Model;

namespace DesignAutomationConsole.Attributes
{
    public static class ParameterAttributeExtension
    {
        public static Parameter ToParameter(this ParameterInputAttribute parameterInput)
        {
            return new Parameter()
            {
                LocalName = parameterInput.LocalName,
                Description = parameterInput.Description,
                Verb = Verb.Get,
                Required = parameterInput.Required,
            };
        }

        public static Parameter ToParameter(this ParameterOutputAttribute parameterOutput)
        {
            return new Parameter()
            {
                LocalName = parameterOutput.LocalName,
                Description = parameterOutput.Description,
                Verb = Verb.Put,
                Zip = parameterOutput.Zip,
            };
        }
    }
}