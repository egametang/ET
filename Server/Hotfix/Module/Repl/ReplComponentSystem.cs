using System;
using System.Threading;
using System.Threading.Tasks;
using ETModel;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace ETHotfix
{
    [ObjectSystem]
    public class ReplComponentStartSystem : StartSystem<ReplComponent>
    {
        public override void Start(ReplComponent self)
        {
            self.ScriptOptions = ScriptOptions.Default
                    .WithMetadataResolver(ScriptMetadataResolver.Default.WithBaseDirectory(Environment.CurrentDirectory))
                    .AddReferences(typeof (ReplComponent).Assembly)
                    .AddImports("System");

            self.Run().NoAwait();
        }
    }
    
    [ObjectSystem]
    public class ReplComponentLoadSystem : LoadSystem<ReplComponent>
    {
        public override void Load(ReplComponent self)
        {
            self.CancellationTokenSource?.Cancel();
            self.ScriptState = null;
            self.Run().NoAwait();
        }
    }

    public static class ReplComponentHelper
    {
        public static async ETVoid Run(this ReplComponent self)
        {
            self.CancellationTokenSource = new CancellationTokenSource();
            
            while (true)
            {
                try
                {
                    string line = await Task.Factory.StartNew(() =>
                    {
                        Console.Out.Write("> ");
                        return Console.In.ReadLine();
                    }, self.CancellationTokenSource.Token);
                    
                    line = line.Trim();
                    
                    if (line == "exit")
                    {
                        self.ScriptState = null;
                        continue;
                    }
    
                    if (self.ScriptState == null)
                    {
                        self.ScriptState = await CSharpScript.RunAsync(line, self.ScriptOptions, cancellationToken: self.CancellationTokenSource.Token);
                    }
                    else
                    {
                        self.ScriptState = await self.ScriptState.ContinueWithAsync(line, cancellationToken: self.CancellationTokenSource.Token);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}