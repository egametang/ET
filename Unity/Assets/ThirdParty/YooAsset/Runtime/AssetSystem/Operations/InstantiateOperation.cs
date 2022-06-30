using UnityEngine;

namespace YooAsset
{
	public sealed class InstantiateOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			Clone,
			Done,
		}

		private readonly AssetOperationHandle _handle;
		private readonly Vector3 _position;
		private readonly Quaternion _rotation;
		private readonly Transform _parent;
		private readonly bool _setPositionRotation;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 实例化的游戏对象
		/// </summary>
		public GameObject Result = null;


		internal InstantiateOperation(AssetOperationHandle handle, Vector3 position, Quaternion rotation, Transform parent, bool setPositionRotation)
		{
			_handle = handle;
			_position = position;
			_rotation = rotation;
			_parent = parent;
			_setPositionRotation = setPositionRotation;
		}
		internal override void Start()
		{
			_steps = ESteps.Clone;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.Clone)
			{
				if (_handle.IsValid == false)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"{nameof(AssetOperationHandle)} is invalid.";
					return;
				}

				if (_handle.IsDone == false)
					return;

				if (_handle.AssetObject == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"{nameof(AssetOperationHandle.AssetObject)} is null.";
					return;
				}

				if(_setPositionRotation)
				{
					if (_parent == null)
						Result = Object.Instantiate(_handle.AssetObject as GameObject, _position, _rotation);
					else
						Result = Object.Instantiate(_handle.AssetObject as GameObject, _position, _rotation, _parent);
				}
				else
				{
					if (_parent == null)
						Result = Object.Instantiate(_handle.AssetObject as GameObject);
					else
						Result = Object.Instantiate(_handle.AssetObject as GameObject, _parent);
				}

				_steps = ESteps.Done;
				Status = EOperationStatus.Succeed;
			}
		}
	}
}