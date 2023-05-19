using System;
using System.Collections.Generic;

namespace ET.Generator
{

    public class AttributeTemplate
    {
        private Dictionary<string, string> templates = new Dictionary<string, string>();

        public AttributeTemplate()
        {
            this.templates.Add("EntitySystem", $$"""
        $attribute$
                public class $argsTypesUnderLine$_$methodName$System: $methodName$System<$argsTypes$>
                {   
                    protected override void $methodName$($argsTypeVars$)
                    {
                        self.$methodName$($argsVars$);
                    }
                }
        """);
        }

        public string Get(string attributeType)
        {
            if (!this.templates.TryGetValue(attributeType, out string template))
            {
                throw new Exception($"not config template: {attributeType}");
            }

            if (template == null)
            {
                throw new Exception($"not config template: {attributeType}");
            }

            return template;
        }
    }
}