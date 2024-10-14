using JetBrains.Annotations;
using UnityEngine;
#if TEST_FRAMEWORK
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
#else
using System;
#endif

namespace Packages.Rider.Editor.UnitTesting
{
  /// <summary>
  /// Is called by Rider Unity plugin via reflections
  /// </summary>
  [UsedImplicitly]
  public static class RiderTestRunner
  {
#if TEST_FRAMEWORK
    private static readonly TestsCallback Callback = ScriptableObject.CreateInstance<TestsCallback>();
    private static string _sessionGuid;
#endif
    
    /// <summary>
    /// Is called by Rider Unity plugin via reflections
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="testMode"></param>
    /// <param name="assemblyNames"></param>
    /// <param name="testNames"></param>
    /// <param name="categoryNames"></param>
    /// <param name="groupNames"></param>
    /// <param name="buildTarget"></param>
    /// <param name="callbacksHandlerCodeBase"></param>
    /// <param name="callbacksHandlerTypeName"></param>
    /// <param name="callbacksHandlerDependencies"></param>
    [UsedImplicitly]
    public static void RunTestsWithSyncCallbacks(string sessionId, int testMode, string[] assemblyNames, 
      string[] testNames, string[] categoryNames, string[] groupNames, int? buildTarget,  
      string callbacksHandlerCodeBase, string callbacksHandlerTypeName, string[] callbacksHandlerDependencies)
    {
#if !TEST_FRAMEWORK
      Debug.LogError("Update Test Framework package to v.1.1.8+ to run tests from Rider.");
      throw new NotSupportedException("Incompatible `Test Framework` package in Unity. Update to v.1.1.8+");
#else
      SyncTestRunEventsHandler.instance.InitRun(sessionId, callbacksHandlerCodeBase, callbacksHandlerTypeName, callbacksHandlerDependencies);
      RunTests(testMode, assemblyNames, testNames, categoryNames, groupNames, buildTarget);
#endif      
    }
    
    /// <summary>
    /// Is called by Rider Unity plugin via reflections
    /// </summary>
    /// <param name="testMode"></param>
    /// <param name="assemblyNames"></param>
    /// <param name="testNames"></param>
    /// <param name="categoryNames"></param>
    /// <param name="groupNames"></param>
    /// <param name="buildTarget"></param>
    [UsedImplicitly]
    public static void RunTests(int testMode, string[] assemblyNames, string[] testNames, string[] categoryNames, string[] groupNames, int? buildTarget)
    {
#if !TEST_FRAMEWORK
      Debug.LogError("Update Test Framework package to v.1.1.8+ to run tests from Rider.");
      throw new NotSupportedException("Incompatible `Test Framework` package in Unity. Update to v.1.1.8+");
#else
      CallbackData.instance.isRider = true;
            
      var api = ScriptableObject.CreateInstance<TestRunnerApi>();
      var settings = new ExecutionSettings();
      var filter = new Filter
      {
        assemblyNames = assemblyNames,
        testNames = testNames,
        categoryNames = categoryNames,
        groupNames = groupNames,
        targetPlatform = (BuildTarget?) buildTarget
      };

      if (testMode > 0) // for future use - test-framework would allow running both Edit and Play test at once
      {
          filter.testMode = (TestMode) testMode;
      }
      
      api.RetrieveTestList(filter.testMode, adaptor =>
      {
        // start tests if there any, otherwise send a RunFinished signal // see RIDER-91705
        if (adaptor.Children.Any(a => a.IsTestAssembly && assemblyNames.Contains(Path.GetFileNameWithoutExtension(a.Name))))
        {
          settings.filters = new[]
          {
            filter
          };

          _sessionGuid = api.Execute(settings);

          api.UnregisterCallbacks(Callback); // avoid multiple registrations
          api.RegisterCallbacks(Callback); // receive information about when the test suite and individual tests starts and stops.
        }
        else
        {
          CallbackData.instance.isRider = false;

          CallbackData.instance.events.Add(
            new TestEvent(EventType.RunFinished, "", "", "", 0, NUnit.Framework.Interfaces.TestStatus.Inconclusive, ""));
          CallbackData.instance.RaiseChangedEvent();
        }
      });

#endif
    }

    [UsedImplicitly]
    internal static void CancelTestRun()
    {
#if !TEST_FRAMEWORK
      Debug.LogError("Update Test Framework package to v.1.1.8+ to run tests from Rider.");
      throw new NotSupportedException("Incompatible `Test Framework` package in Unity. Update to v.1.1.8+");
#else
      var methodInfo = typeof(TestRunnerApi).GetMethod("CancelTestRun");
      if (methodInfo == null)
        methodInfo = typeof(TestRunnerApi).GetMethod("CancelTestRun", BindingFlags.Static | BindingFlags.NonPublic);
      methodInfo.Invoke(null, new object[] { _sessionGuid });
#endif
    }
  }
}