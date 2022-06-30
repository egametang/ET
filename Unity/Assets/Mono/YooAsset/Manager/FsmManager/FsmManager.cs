using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 有限状态机
/// </summary>
public static class FsmManager
{
	private static readonly List<IFsmNode> _nodes = new List<IFsmNode>();
	private static IFsmNode _curNode;
	private static IFsmNode _preNode;

	/// <summary>
	/// 当前运行的节点名称
	/// </summary>
	public static string CurrentNodeName
	{
		get { return _curNode != null ? _curNode.Name : string.Empty; }
	}

	/// <summary>
	/// 之前运行的节点名称
	/// </summary>
	public static string PreviousNodeName
	{
		get { return _preNode != null ? _preNode.Name : string.Empty; }
	}


	/// <summary>
	/// 启动状态机
	/// </summary>
	/// <param name="entryNode">入口节点</param>
	public static void Run(string entryNode)
	{
		_curNode = GetNode(entryNode);
		_preNode = GetNode(entryNode);

		if (_curNode != null)
			_curNode.OnEnter();
		else
			UnityEngine.Debug.LogError($"Not found entry node : {entryNode}");
	}

	/// <summary>
	/// 更新状态机
	/// </summary>
	public static void Update()
	{
		if (_curNode != null)
			_curNode.OnUpdate();
	}

	/// <summary>
	/// 加入一个节点
	/// </summary>
	public static void AddNode(IFsmNode node)
	{
		if (node == null)
			throw new ArgumentNullException();

		if (_nodes.Contains(node) == false)
		{
			_nodes.Add(node);
		}
		else
		{
			UnityEngine.Debug.LogWarning($"Node {node.Name} already existed");
		}
	}

	/// <summary>
	/// 转换节点
	/// </summary>
	public static void Transition(string nodeName)
	{
		if (string.IsNullOrEmpty(nodeName))
			throw new ArgumentNullException();

		IFsmNode node = GetNode(nodeName);
		if (node == null)
		{
			UnityEngine.Debug.LogError($"Can not found node {nodeName}");
			return;
		}

		UnityEngine.Debug.Log($"FSM change {_curNode.Name} to {node.Name}");
		_preNode = _curNode;
		_curNode.OnExit();
		_curNode = node;
		_curNode.OnEnter();
	}

	/// <summary>
	/// 返回到之前的节点
	/// </summary>
	public static void RevertToPreviousNode()
	{
		Transition(PreviousNodeName);
	}

	private static bool IsContains(string nodeName)
	{
		for (int i = 0; i < _nodes.Count; i++)
		{
			if (_nodes[i].Name == nodeName)
				return true;
		}
		return false;
	}
	private static IFsmNode GetNode(string nodeName)
	{
		for (int i = 0; i < _nodes.Count; i++)
		{
			if (_nodes[i].Name == nodeName)
				return _nodes[i];
		}
		return null;
	}
}