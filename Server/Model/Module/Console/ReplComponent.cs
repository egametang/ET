using System;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ET
{
    public class ReplComponentAwakeSystem : AwakeSystem<ReplComponent>
    {
        public override void Awake(ReplComponent self)
        {
            self.ScriptOptions = ScriptOptions.Default
                    .WithMetadataResolver(ScriptMetadataResolver.Default.WithBaseDirectory(Environment.CurrentDirectory))
                    .AddReferences(typeof (ReplComponent).Assembly)
                    .AddImports("System");
        }
    }
    
    public class ReplComponent: Entity
    {
        public ScriptOptions ScriptOptions;
        public ScriptState ScriptState;

        public async ETTask<bool> Run(string line, CancellationToken cancellationToken)
        {
            switch (line)
            {
                case "exit":
                {
                    this.Parent.RemoveComponent<ReplComponent>();
                    return true;
                }
                case "reset":
                {
                    this.ScriptState = null;
                    return false;
                }
                default:
                {
                    try
                    {
                        if (this.ScriptState == null)
                        {
                            this.ScriptState = await CSharpScript.RunAsync(line, this.ScriptOptions, cancellationToken: cancellationToken);
                        }
                        else
                        {
                            this.ScriptState = await this.ScriptState.ContinueWithAsync(line, cancellationToken: cancellationToken);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    return false;
                }
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            base.Dispose();
            this.ScriptOptions = null;
            this.ScriptState = null;
        }
    }
}