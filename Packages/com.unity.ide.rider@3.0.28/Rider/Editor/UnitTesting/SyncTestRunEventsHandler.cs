#if TEST_FRAMEWORK
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Packages.Rider.Editor.UnitTesting
{
  internal class SyncTestRunEventsHandler : ScriptableSingleton<SyncTestRunEventsHandler>
  {
    [SerializeField] private string m_SessionId;
    [SerializeField] private string m_HandlerCodeBase;
    [SerializeField] private string m_HandlerTypeName;
    [SerializeField] private string[] m_HandlerDependencies;
    [SerializeField] private bool m_RunInitialized;
    
    private object m_Handler;
    private MethodInfo m_OnSessionStartedMethodInfo;
    private MethodInfo m_OnTestStartedMethodInfo;
    private MethodInfo m_OnTestFinishedMethodInfo;
    private MethodInfo m_OnSessionFinishedMethodInfo;
    
    internal void InitRun(string sessionId, string handlerCodeBase, string handlerTypeName, string[] handlerDependencies)
    {
      if (PluginSettings.SelectedLoggingLevel >= LoggingLevel.TRACE)
        Debug.Log("Rider Test Runner: initializing sync callbacks handler: " + 
                  $"sessionId={sessionId}, " + 
                  $"codeBase={handlerCodeBase}, " + 
                  $"typeName={handlerTypeName}, " + 
                  $"dependencies={(handlerDependencies == null ? "" : string.Join("; ", handlerDependencies))}");
    
      m_SessionId = sessionId;
      m_HandlerCodeBase = handlerCodeBase;
      m_HandlerTypeName = handlerTypeName;
      m_HandlerDependencies = handlerDependencies;
      m_RunInitialized = true;
      
      CreateHandlerInstance();
      SafeInvokeHandlerMethod(m_OnSessionStartedMethodInfo, Array.Empty<object>());
    }

    private void OnEnable()
    {
      if (m_RunInitialized)
        CreateHandlerInstance();
    }

    internal void OnTestStarted(string testId)
    {
      if (m_RunInitialized)
        SafeInvokeHandlerMethod(m_OnTestStartedMethodInfo, new object[] {testId});
    }

    internal void OnTestFinished()
    {
      if (m_RunInitialized)
        SafeInvokeHandlerMethod(m_OnTestFinishedMethodInfo, Array.Empty<object>());
    }

    internal void OnRunFinished()
    {
      if (!m_RunInitialized)
        return;
      
      SafeInvokeHandlerMethod(m_OnSessionFinishedMethodInfo, Array.Empty<object>());
      CleanUp();
      m_RunInitialized = false;
    }

    private void SafeInvokeHandlerMethod(MethodInfo methodInfo, object[] args)
    {
      try
      {
        methodInfo?.Invoke(m_Handler, args);
      }
      catch (Exception e)
      {
        Debug.LogException(e);
      }
    }
    
    private void CreateHandlerInstance()
    {
      try
      {
        if (m_HandlerDependencies != null)
          foreach (var dependency in m_HandlerDependencies)
          {
            if (PluginSettings.SelectedLoggingLevel >= LoggingLevel.TRACE) 
              Debug.Log($"Rider Test Runner: loading assembly from {dependency}");
            Assembly.LoadFrom(dependency);
          }
        if (PluginSettings.SelectedLoggingLevel >= LoggingLevel.TRACE)
          Debug.Log($"Rider Test Runner: loading assembly from {m_HandlerCodeBase}");
        var assembly = Assembly.LoadFrom(m_HandlerCodeBase);
        var type = assembly.GetType(m_HandlerTypeName);
        if (type == null)
        {
          Debug.LogError($"Rider Test Runner: type '{m_HandlerTypeName}' not found in assembly '{assembly.FullName}'");
          return;
        }
        
        if (PluginSettings.SelectedLoggingLevel >= LoggingLevel.TRACE) 
          Debug.Log($"Rider Test Runner: creating instance of type '{type.AssemblyQualifiedName}'");
        m_Handler = Activator.CreateInstance(type, m_SessionId);

        m_OnSessionStartedMethodInfo = type.GetMethod("OnSessionStarted", BindingFlags.Instance | BindingFlags.Public);
        if (m_OnSessionStartedMethodInfo == null)
        {
          Debug.LogError($"Rider Test Runner: OnSessionStarted method not found in type='{type.AssemblyQualifiedName}'");
          return;
        }

        m_OnTestStartedMethodInfo = type.GetMethod("OnTestStarted", BindingFlags.Instance | BindingFlags.Public);
        if (m_OnTestStartedMethodInfo == null)
        {
          Debug.LogError($"Rider Test Runner: OnTestStarted method not found in type='{type.AssemblyQualifiedName}'");
          return;
        }

        m_OnTestFinishedMethodInfo = type.GetMethod("OnTestFinished", BindingFlags.Instance | BindingFlags.Public);
        if (m_OnTestFinishedMethodInfo == null)
        {
          Debug.LogError($"Rider Test Runner: OnTestFinished method not found in type='{type.AssemblyQualifiedName}'");
          return;
        }

        m_OnSessionFinishedMethodInfo = type.GetMethod("OnSessionFinished", BindingFlags.Instance | BindingFlags.Public);
        if (m_OnSessionFinishedMethodInfo == null)
          Debug.LogError($"Rider Test Runner: OnSessionFinished method not found in type='{type.AssemblyQualifiedName}'");
      }
      catch (Exception e)
      {
        Debug.LogException(e);
      }
    }

    private void CleanUp()
    {
      m_Handler = null;
      m_OnSessionStartedMethodInfo = null;
      m_OnSessionFinishedMethodInfo = null;
      m_OnTestStartedMethodInfo = null;
      m_OnTestFinishedMethodInfo = null;
    }
  }
}
#endif