using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 文字打字效果。先调用Start，然后Print。
	/// </summary>
	public class TypingEffect
	{
		protected TextField _textField;

		protected List<Vector3> _backupVerts;
		protected bool _stroke;
		protected bool _shadow;

		protected int _printIndex;
		protected int _mainLayerStart;
		protected int _strokeLayerStart;
		protected int _vertIndex;
		protected int _mainLayerVertCount;

		protected bool _started;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="textField"></param>
		public TypingEffect(TextField textField)
		{
			_textField = textField;
			_textField.EnableCharPositionSupport();
			_backupVerts = new List<Vector3>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="textField"></param>
		public TypingEffect(GTextField textField)
		{
			if (textField is GRichTextField)
				_textField = ((RichTextField)textField.displayObject).textField;
			else
				_textField = (TextField)textField.displayObject;
			_textField.EnableCharPositionSupport();
			_backupVerts = new List<Vector3>();
		}

		/// <summary>
		/// 开始打字效果。可以重复调用重复启动。
		/// </summary>
		public void Start()
		{
			_textField.graphics.meshModifier -= OnMeshModified;
			_textField.Redraw();
			_textField.graphics.meshModifier += OnMeshModified;

			_backupVerts.Clear();
			_stroke = false;
			_shadow = false;
			_mainLayerStart = 0;
			_mainLayerVertCount = 0;
			_printIndex = 0;
			_vertIndex = 0;
			_started = true;

			int vertCount = _textField.graphics.vertCount;

			//备份所有顶点后将顶点置0
			Vector3[] vertices = _textField.graphics.vertices;
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < vertCount; i++)
			{
				_backupVerts.Add(vertices[i]);
				vertices[i] = zero;
			}
			_textField.graphics.mesh.vertices = vertices;

			//隐藏所有混排的对象
			if (_textField.richTextField != null)
			{
				int ec = _textField.richTextField.htmlElementCount;
				for (int i = 0; i < ec; i++)
					_textField.richTextField.ShowHtmlObject(i, false);
			}

			int charCount = _textField.charPositions.Count;
			for (int i = 0; i < charCount; i++)
			{
				TextField.CharPosition cp = _textField.charPositions[i];
				if (cp.vertCount > 0)
					_mainLayerVertCount += cp.vertCount;
			}

			if (_mainLayerVertCount < vertCount) //说明有描边或者阴影
			{
				if (vertCount == _mainLayerVertCount * 6)
				{
					_stroke = true;
					_shadow = true;
					_mainLayerStart = vertCount * 5 / 6;
					_strokeLayerStart = vertCount / 6;
				}
				else if (vertCount == _mainLayerVertCount * 5)
				{
					_stroke = true;
					_shadow = false;
					_mainLayerStart = vertCount * 4 / 5;
					_strokeLayerStart = 0;
				}
				else if (vertCount == _mainLayerVertCount * 2)
				{
					_stroke = false;
					_shadow = true;
					_mainLayerStart = vertCount / 2;
				}
			}
		}

		/// <summary>
		/// 输出一个字符。如果已经没有剩余的字符，返回false。
		/// </summary>
		/// <returns></returns>
		public bool Print()
		{
			if (!_started)
				return false;

			TextField.CharPosition cp;
			List<TextField.CharPosition> charPositions = _textField.charPositions;
			int listCnt = charPositions.Count;

			while (_printIndex < listCnt - 1) //最后一个是占位的，无效的，所以-1
			{
				cp = charPositions[_printIndex++];
				if (cp.vertCount < 0) //这是一个图片
				{
					_textField.richTextField.ShowHtmlObject(-cp.vertCount - 1, true);
					return true;
				}
				else if (!char.IsWhiteSpace(_textField.parsedText[_printIndex - 1]))
				{
					if (cp.vertCount > 0)
						output(cp.vertCount);
					return true;
				}
				else if (cp.vertCount > 0) //空白
					output(cp.vertCount);
			}

			Cancel();
			return false;
		}

		private void output(int vertCount)
		{
			Vector3[] vertices = _textField.graphics.vertices;
			int start, end;

			start = _mainLayerStart + _vertIndex;
			end = start + vertCount;
			for (int i = start; i < end; i++)
			{
				vertices[i] = _backupVerts[i];
				_backupVerts[i] = Vector3.zero;
			}

			if (_stroke)
			{
				start = _strokeLayerStart + _vertIndex;
				end = start + vertCount;
				for (int i = start; i < end; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						int k = i + _mainLayerVertCount * j;
						vertices[k] = _backupVerts[k];
						_backupVerts[k] = Vector3.zero;
					}
				}
			}

			if (_shadow)
			{
				start = _vertIndex;
				end = start + vertCount;
				for (int i = start; i < end; i++)
				{
					vertices[i] = _backupVerts[i];
					_backupVerts[i] = Vector3.zero;
				}
			}

			_textField.graphics.mesh.vertices = vertices;

			_vertIndex += vertCount;
		}

		/// <summary>
		/// 打印的协程。
		/// </summary>
		/// <param name="interval">每个字符输出的时间间隔</param>
		/// <returns></returns>
		public IEnumerator Print(float interval)
		{
			while (Print())
				yield return new WaitForSeconds(interval);
		}

		/// <summary>
		/// 使用固定时间间隔完成整个打印过程。
		/// </summary>
		/// <param name="interval"></param>
		public void PrintAll(float interval)
		{
			Timers.inst.StartCoroutine(Print(interval));
		}

		public void Cancel()
		{
			if (!_started)
				return;

			_started = false;
			_textField.graphics.meshModifier -= OnMeshModified;
		}

		/// <summary>
		/// 当打字过程中，文本可能会由于字体纹理更改而发生字体重建，要处理这种情况。
		/// 图片对象不需要处理，因为HtmlElement.status里设定的隐藏标志不会因为Mesh更新而被冲掉。
		/// </summary>
		void OnMeshModified()
		{
			Vector3[] vertices = _textField.graphics.vertices;

			if (vertices.Length != _backupVerts.Count) //可能文字都改了
			{
				Cancel();
				return;
			}

			int vertCount = vertices.Length;
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < vertCount; i++)
			{
				if (_backupVerts[i] != zero) //not output yet
				{
					_backupVerts[i] = vertices[i];
					vertices[i] = Vector3.zero;
				}
			}
		}
	}
}
