#if TEST_FRAMEWORK
using System;
using System.Linq;
using System.Text;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Packages.Rider.Editor.UnitTesting
{
  internal class TestsCallback : ScriptableObject, IErrorCallbacks
    {
        public void RunFinished(ITestResultAdaptor result)
        {
          CallbackData.instance.isRider = false;
          
          CallbackData.instance.events.Add(
            new TestEvent(EventType.RunFinished, "", "","", 0, ParseTestStatus(result.TestStatus), ""));
          CallbackData.instance.RaiseChangedEvent();
        }
        
        public void RunStarted(ITestAdaptor testsToRun)
        {
          CallbackData.instance.events.Add(
            new TestEvent(EventType.RunStarted, "", "","", 0, NUnit.Framework.Interfaces.TestStatus.Passed, ""));
          CallbackData.instance.RaiseChangedEvent();
        }

        public void TestStarted(ITestAdaptor result)
        {
          // RIDER-69927 "Test not run" status is shown for the test suite when running unit tests for Unity project
          var method = result.Method ?? result.Children.Select(a=>a.Method).FirstOrDefault(b => b != null);
          if (method == null) return;

          CallbackData.instance.events.Add(
           new TestEvent(EventType.TestStarted, GenerateId(result), method.TypeInfo.Assembly.GetName().Name, "", 0, NUnit.Framework.Interfaces.TestStatus.Passed, GenerateId(result.Parent)));
          CallbackData.instance.RaiseChangedEvent();
        }

        public void TestFinished(ITestResultAdaptor result)
        {
          var method = result.Test.Method ?? result.Children.Select(a=>a.Test.Method).FirstOrDefault(b => b != null);
          if (method == null) return;
          
          CallbackData.instance.events.Add(
            new TestEvent(EventType.TestFinished, GenerateId(result.Test), method.TypeInfo.Assembly.GetName().Name, ExtractOutput(result), (result.EndTime-result.StartTime).Milliseconds, ParseTestStatus(result.TestStatus), GenerateId(result.Test.Parent)));
          CallbackData.instance.RaiseChangedEvent();
        }

        public void OnError(string message)
        {
          CallbackData.instance.isRider = false;
          
          CallbackData.instance.events.Add(
            new TestEvent(EventType.RunFinished, "", "",message, 0, NUnit.Framework.Interfaces.TestStatus.Failed, ""));
          CallbackData.instance.RaiseChangedEvent();
        }

        // see explanation in https://jetbrains.team/p/net/code/dotnet-libs/files/f04cde7d1dd70ee13bf5532e30f929b9b5ed08a4/ReSharperTestRunner/src/Adapters/TestRunner.Adapters.NUnit3/RemoteTaskDepot.cs?tab=source&line=129
        private static string GenerateId(ITestAdaptor node)
        {
          // ES: Parameterized tests defined in a parametrized test fixture, though 
          // constructed for a particular test fixture with the given parameter, have identical fullname that does
          // not include parameters of parent testfixture (name of the without parameters is used instead).
          // This leads to 'Test with {id} id is already running' message.
          if (node.TypeInfo == null) 
            return $"{node.Parent.FullName}.{node.Name}";

          return node.FullName;
        }

        private static NUnit.Framework.Interfaces.TestStatus ParseTestStatus(TestStatus testStatus)
        {
          return (NUnit.Framework.Interfaces.TestStatus)Enum.Parse(typeof(NUnit.Framework.Interfaces.TestStatus), testStatus.ToString());
        }
        
        private static string ExtractOutput(ITestResultAdaptor testResult)
        {
          var stringBuilder = new StringBuilder();
          if (testResult.Message != null)
          {
            stringBuilder.AppendLine("Message: ");
            stringBuilder.AppendLine(testResult.Message);
          }

          if (!string.IsNullOrEmpty(testResult.Output))
          {
            stringBuilder.AppendLine("Output: ");
            stringBuilder.AppendLine(testResult.Output);
          }

          if (!string.IsNullOrEmpty(testResult.StackTrace))
          {
            stringBuilder.AppendLine("Stacktrace: ");
            stringBuilder.AppendLine(testResult.StackTrace);
          }
      
          var result = stringBuilder.ToString();
          if (result.Length > 0)
            return result;

          return testResult.Output ?? string.Empty;
        }
    }
}
#endif