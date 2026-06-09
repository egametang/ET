/*using System;
using System.Collections.Generic;
using System.Reflection;

using ET.Test;

namespace ET
{
    internal sealed class UnityBridgeUnityTestRunHandler : AUnityBridgeHandler<UnityTestRunRequest, UnityTestRunResponse>
    {
        protected override async ETTask<IResponse> Run(UnityTestRunRequest command)
        {
            string pattern = string.IsNullOrWhiteSpace(command.Name) ? ".*" : command.Name.Trim();
            long startedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            UnityTestRunResponse response = UnityTestRunResponse.Create();
            response.Name = pattern;

            bool ownsCodeTypes = false;
            try
            {
                if (CodeTypes.Instance == null)
                {
                    List<Assembly> assemblies = new();
                    HashSet<string> assemblyNames = new(StringComparer.Ordinal)
                    {
                        "ET.Core",
                        "ET.Loader",
                        "ET.Model",
                        "ET.ModelView",
                        "ET.Editor",
                        "ET.Hotfix",
                        "ET.HotfixView"
                    };

                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        string assemblyName = assembly.GetName().Name;
                        if (!assemblyNames.Contains(assemblyName))
                        {
                            continue;
                        }

                        assemblies.Add(assembly);
                    }

                    World.Instance.AddSingleton<CodeTypes, Assembly[]>(assemblies.ToArray());
                    ownsCodeTypes = true;
                }

                TestDispatcher dispatcher = new();
                dispatcher.Awake();
                List<TestCaseInfo> testCases = dispatcher.Get(pattern);
                if (testCases.Count == 0)
                {
                    response.Error = UnityBridgeErrorCode.HandlerFail;
                    response.Message = "not found test";
                    return response;
                }

                TestArgs args = new()
                {
                    Name = pattern
                };

                foreach (TestCaseInfo testCase in testCases)
                {
                    BridgeTestResult result = await RunTest(testCase, args);
                    response.Results.Add(result);
                    if (result.Passed)
                    {
                        ++response.Passed;
                    }
                    else
                    {
                        ++response.Failed;
                    }
                }

                response.Matched = testCases.Count;
                response.DurationMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startedAt;
                if (response.Failed > 0)
                {
                    response.Error = UnityBridgeErrorCode.HandlerFail;
                    response.Message = $"unity tests failed: {response.Failed}/{response.Matched}";
                }
                else
                {
                    response.Message = "unity tests completed";
                }

                return response;
            }
            finally
            {
                if (ownsCodeTypes)
                {
                    World.Instance.RemoveSingleton<CodeTypes>();
                }
            }
        }

        private static async ETTask<BridgeTestResult> RunTest(TestCaseInfo testCase, TestArgs args)
        {
            long startedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            BridgeTestResult result = BridgeTestResult.Create();
            result.Name = testCase.Name;
            result.FullName = testCase.Handler.GetType().FullName;

            try
            {
                int error = await testCase.Handler.Handle(new TestContext { Args = args });
                result.Error = error;
                result.Passed = error == ErrorCode.ERR_Success;
                result.Message = result.Passed ? "success" : $"failed with error: {error}";
            }
            catch (Exception e)
            {
                result.Error = UnityBridgeErrorCode.HandlerFail;
                result.Passed = false;
                result.Message = e.ToString();
            }
            finally
            {
                result.DurationMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startedAt;
            }

            return result;
        }
    }
}
*/
