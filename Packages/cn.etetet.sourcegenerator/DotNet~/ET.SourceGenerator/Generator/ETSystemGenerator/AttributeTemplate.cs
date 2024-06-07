using System;
using System.Collections.Generic;

namespace ET.Generator
{

    public class AttributeTemplate
    {
        private Dictionary<string, string> templates = new Dictionary<string, string>();

        public AttributeTemplate()
        {
            this.templates.Add("EntitySystem", 
                $$"""
                $attribute$
                        public class $argsTypesUnderLine$_$methodName$System: $methodName$System<$argsTypes$>
                        {   
                            protected override $returnType$ $methodName$($argsTypesVars$)
                            {
                                $return$$argsVars0$.$methodName$($argsVarsWithout0$);
                            }
                        }
                """);
            
            this.templates.Add("LSEntitySystem", 
                $$"""
                $attribute$
                        public class $argsTypesUnderLine$_$methodName$System: $methodName$System<$argsTypes$>
                        {   
                            protected override void $methodName$($argsTypesVars$)
                            {
                                $argsVars0$.$methodName$($argsVarsWithout0$);
                            }
                        }
                """);
            
            this.templates.Add("MessageHandler", 
                $$"""
                $attribute$
                	    public class $className$_$methodName$_Handler: MessageHandler<$argsTypesWithout0$>
                	    {
                	    	protected override async ETTask Run($argsTypesVars$)
                	    	{
                                await $className$.$methodName$($argsVars$);
                            }
                        }
                """);
            
            this.templates.Add("ActorMessageHandler", 
                $$"""
                $attribute$
                	    public class $className$_$methodName$_Handler: ActorMessageHandler<$argsTypes$>
                	    {
                	    	protected override async ETTask Run($argsTypesVars$)
                	    	{
                                await $className$.$methodName$($argsVars$);
                            }
                        }
                """);
            
            this.templates.Add("ActorMessageLocationHandler", 
                $$"""
                $attribute$
                	    public class $className$_$methodName$_Handler: ActorMessageLocationHandler<$argsTypes$>
                	    {
                	    	protected override async ETTask Run($argsTypesVars$)
                	    	{
                                await $className$.$methodName$($argsVars$);
                            }
                        }
                """);
            
            this.templates.Add("Event", 
                $$"""
                $attribute$
                        public class $argsTypes2$_$methodName$: AEvent<$argsTypes$>
                        {
                            protected override async ETTask Run($argsTypesVars$)
                            {
                                await $className$.$methodName$($argsVars$);
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

        public bool Contains(string attributeType)
        {
            return this.templates.ContainsKey(attributeType);
        }
    }
}