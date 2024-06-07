#if TEST_FRAMEWORK
using NUnit.Framework.Interfaces;
using Packages.Rider.Editor.UnitTesting;
using UnityEngine.TestRunner;

[assembly: TestRunCallback(typeof(SyncTestRunCallback))]

namespace Packages.Rider.Editor.UnitTesting
{
  internal class SyncTestRunCallback : ITestRunCallback
  {
    public void RunStarted(ITest testsToRun)
    {
    }

    public void RunFinished(ITestResult testResults)
    {
      SyncTestRunEventsHandler.instance.OnRunFinished();
    }

    public void TestStarted(ITest test)
    {
      if (!test.IsSuite)
        SyncTestRunEventsHandler.instance.OnTestStarted(GenerateId(test));
    }

    public void TestFinished(ITestResult result)
    {
      if (!result.Test.IsSuite)
        SyncTestRunEventsHandler.instance.OnTestFinished();
    }
    
    // https://jetbrains.team/p/net/code/dotnet-libs/files/f04cde7d1dd70ee13bf5532e30f929b9b5ed08a4/ReSharperTestRunner/src/Adapters/TestRunner.Adapters.NUnit3/RemoteTaskDepot.cs?tab=source&line=129
    private static string GenerateId(ITest node)
    {
      // ES: Parameterized tests defined in a parametrized test fixture, though 
      // constructed for a particular test fixture with the given parameter, have identical fullname that does
      // not include parameters of parent testfixture (name of the without parameters is used instead).
      // This leads to 'Test with {id} id is already running' message.
      var typeName = node.GetType().Name;
      if (typeName == "ParameterizedMethod" ||
          typeName == "GenericMethod") 
        return $"{node.Parent.FullName}.{node.Name}";
      
      return node.FullName;
    }
  }
}
#endif 