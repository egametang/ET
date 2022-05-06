using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {

//will auto register in unity
#if UNITY_5_3_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        static private void RegisterBindingAction()
        {
            ILRuntime.Runtime.CLRBinding.CLRBindingUtils.RegisterBindingAction(Initialize);
        }

        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector2> s_UnityEngine_Vector2_Binding_Binder = null;
        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector3> s_UnityEngine_Vector3_Binding_Binder = null;
        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Quaternion> s_UnityEngine_Quaternion_Binding_Binder = null;

        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding.Register(app);
            ET_TimeHelper_Binding.Register(app);
            ET_ETAsyncTaskMethodBuilder_Binding.Register(app);
            ET_Log_Binding.Register(app);
            System_String_Binding.Register(app);
            ET_ETTask_1_Boolean_Binding.Register(app);
            ET_ETTask_1_Int32_Binding.Register(app);
            ET_ETTask_1_ILTypeInstance_Binding.Register(app);
            ET_NetworkHelper_Binding.Register(app);
            ET_ListComponent_1_Vector3_Binding.Register(app);
            System_Collections_Generic_List_1_Single_Binding.Register(app);
            UnityEngine_Vector3_Binding.Register(app);
            System_Collections_Generic_List_1_Vector3_Binding.Register(app);
            System_IDisposable_Binding.Register(app);
            UnityEngine_Quaternion_Binding.Register(app);
            ET_ETAsyncTaskMethodBuilder_1_Boolean_Binding.Register(app);
            System_Action_1_Boolean_Binding.Register(app);
            System_Math_Binding.Register(app);
            System_Collections_Generic_List_1_Vector3_Binding_Enumerator_Binding.Register(app);
            ET_ETCancellationToken_Binding.Register(app);
            ET_ETAsyncTaskMethodBuilder_1_Int32_Binding.Register(app);
            ET_ETTask_Binding.Register(app);
            System_IO_MemoryStream_Binding.Register(app);
            System_BitConverter_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_List_1_Int64_Binding.Register(app);
            System_Collections_Generic_List_1_Int64_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_List_1_Int32_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_SortedDictionary_2_Int32_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_SortedDictionary_2_Int32_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_SortedDictionary_2_Int32_ILTypeInstance_Binding_ValueCollection_Binding.Register(app);
            System_Collections_Generic_SortedDictionary_2_Int32_ILTypeInstance_Binding_ValueCollection_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding.Register(app);
            System_Type_Binding.Register(app);
            System_Collections_Generic_List_1_Type_Binding.Register(app);
            System_Collections_Generic_List_1_Type_Binding_Enumerator_Binding.Register(app);
            System_Activator_Binding.Register(app);
            System_Reflection_MemberInfo_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_Object_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Byte_Array_Binding.Register(app);
            System_Threading_Monitor_Binding.Register(app);
            ET_ListComponent_1_Task_Binding.Register(app);
            System_Threading_Tasks_Task_Binding.Register(app);
            System_Collections_Generic_List_1_Task_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_UInt16_List_1_ILTypeInstance_Binding.Register(app);
            ET_TService_Binding.Register(app);
            ET_AService_Binding.Register(app);
            ET_RandomHelper_Binding.Register(app);
            ET_ThreadSynchronizationContext_Binding.Register(app);
            ET_ForeachHelper_Binding.Register(app);
            System_Collections_Generic_HashSet_1_AService_Binding.Register(app);
            System_Object_Binding.Register(app);
            System_Exception_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_List_1_ILTypeInstance_Binding.Register(app);
            ET_TimeInfo_Binding.Register(app);
            ET_RpcException_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Int64_Binding.Register(app);
            System_Func_2_String_Byte_Array_Binding.Register(app);
            ET_Recast_Binding.Register(app);
            ET_MathHelper_Binding.Register(app);
            UnityEngine_Mathf_Binding.Register(app);
            ET_StringHelper_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            UnityEngine_LayerMask_Binding.Register(app);
            ET_InputHelper_Binding.Register(app);
            UnityEngine_Camera_Binding.Register(app);
            UnityEngine_Input_Binding.Register(app);
            UnityEngine_Physics_Binding.Register(app);
            UnityEngine_RaycastHit_Binding.Register(app);
            ET_CodeLoader_Binding.Register(app);
            UnityEngine_AsyncOperation_Binding.Register(app);
            UnityEngine_SceneManagement_SceneManager_Binding.Register(app);
            ET_ETAsyncTaskMethodBuilder_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Transform_Binding.Register(app);
            UnityEngine_Object_Binding.Register(app);
            ET_ETTaskCompleted_Binding.Register(app);
            UnityEngine_Resources_Binding.Register(app);
            ReferenceCollector_Binding.Register(app);
            UnityEngine_UI_Button_Binding.Register(app);
            UnityEngine_Events_UnityEvent_Binding.Register(app);
            UnityEngine_UI_InputField_Binding.Register(app);
            UnityEngine_Transform_Binding.Register(app);
            UnityEngine_Animator_Binding.Register(app);
            UnityEngine_RuntimeAnimatorController_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_AnimationClip_Binding.Register(app);
            UnityEngine_AnimatorControllerParameter_Binding.Register(app);
            System_Collections_Generic_HashSet_1_String_Binding.Register(app);
            UnityEngine_AnimationClip_Binding.Register(app);
            ET_Options_Binding.Register(app);
            System_Collections_Generic_List_1_Action_Binding.Register(app);
            System_Collections_Generic_List_1_Action_Binding_Enumerator_Binding.Register(app);
            System_Action_Binding.Register(app);
            ET_MonoPool_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int64_ILTypeInstance_Binding.Register(app);
            LitJson_JsonMapper_Binding.Register(app);
            System_Collections_Generic_HashSet_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Type_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Type_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_String_Type_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_List_1_Type_Binding.Register(app);
            ET_UnOrderMultiMap_2_Type_Type_Binding.Register(app);
            ET_UnOrderMultiMap_2_Type_Object_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_List_1_Object_Binding.Register(app);
            System_Collections_Generic_List_1_Object_Binding.Register(app);
            System_Reflection_Assembly_Binding.Register(app);
            System_Reflection_AssemblyName_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Assembly_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Assembly_Binding_ValueCollection_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Assembly_Binding_ValueCollection_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_Queue_1_Int64_Binding.Register(app);
            ET_ObjectHelper_Binding.Register(app);
            System_Text_StringBuilder_Binding.Register(app);
            System_Collections_Generic_HashSet_1_Type_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_Int32_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int64_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_Int64_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_HashSet_1_Type_Binding_Enumerator_Binding.Register(app);
            System_Linq_Enumerable_Binding.Register(app);
            System_Collections_Generic_IEnumerable_1_KeyValuePair_2_Type_Int32_Binding.Register(app);
            System_Collections_Generic_IEnumerator_1_KeyValuePair_2_Type_Int32_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_Type_Int32_Binding.Register(app);
            System_Collections_IEnumerator_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_UnOrderMultiMap_2_Type_Object_Binding.Register(app);
            System_DateTime_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_Queue_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Queue_1_ILTypeInstance_Binding.Register(app);
            ProtoBuf_Meta_RuntimeTypeModel_Binding.Register(app);
            ProtoBuf_Meta_TypeModel_Binding.Register(app);
            ProtoBuf_Serializer_Binding.Register(app);
            ET_MultiMap_2_Int64_Int64_Binding.Register(app);
            System_Collections_Generic_SortedDictionary_2_Int64_List_1_Int64_Binding.Register(app);
            UnityEngine_Vector2_Binding.Register(app);
            ET_WrapVector3_Binding.Register(app);
            ET_WrapQuaternion_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding_ValueCollection_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding_ValueCollection_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_Int32_ILTypeInstance_Binding.Register(app);
            System_ValueTuple_3_Int32_Int64_Int32_Binding.Register(app);
            System_Collections_Generic_Queue_1_ValueTuple_3_Int32_Int64_Int32_Binding.Register(app);
            ET_MultiMap_2_Int64_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_SortedDictionary_2_Int64_List_1_ILTypeInstance_Binding.Register(app);
            System_IO_Stream_Binding.Register(app);
            ET_ByteHelper_Binding.Register(app);
            System_ValueTuple_2_UInt16_MemoryStream_Binding.Register(app);
            System_Collections_Generic_HashSet_1_UInt16_Binding.Register(app);
            ET_ILog_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_UInt16_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_UInt16_Type_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_Type_Binding.Register(app);
            ET_ErrorCore_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Int64_Binding.Register(app);
            System_Single_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Object_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Object_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_String_Object_Binding.Register(app);
            UnityEngine_TextAsset_Binding.Register(app);
            UnityEngine_AssetBundle_Binding.Register(app);
            ET_ETAsyncTaskMethodBuilder_1_AssetBundle_Binding.Register(app);
            ET_ETAsyncTaskMethodBuilder_1_Object_Array_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_String_Binding.Register(app);
            System_Int32_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_String_Binding.Register(app);
            UnityEngine_AssetBundleCreateRequest_Binding.Register(app);
            ET_ETTask_1_AssetBundle_Binding.Register(app);
            UnityEngine_AssetBundleRequest_Binding.Register(app);
            ET_ETTask_1_Object_Array_Binding.Register(app);
            System_Array_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_String_Array_Binding.Register(app);
            ET_Define_Binding.Register(app);
            UnityEngine_AssetBundleManifest_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Int32_Binding.Register(app);
            System_Collections_Generic_List_1_String_Binding.Register(app);
            System_Collections_Generic_List_1_String_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Dictionary_2_String_Object_Binding.Register(app);
            System_IO_Path_Binding.Register(app);
            ET_PathHelper_Binding.Register(app);
            System_IO_File_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding_ValueCollection_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding_ValueCollection_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_String_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_String_Int32_Binding.Register(app);
            ET_ListComponent_1_ILTypeInstance_Binding.Register(app);
            ET_ListComponent_1_ETTask_Binding.Register(app);
            System_Collections_Generic_List_1_ETTask_Binding.Register(app);
            ET_ETTaskHelper_Binding.Register(app);
            ET_ListComponent_1_String_Binding.Register(app);
            UnityEngine_Component_Binding.Register(app);

            ILRuntime.CLR.TypeSystem.CLRType __clrType = null;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Vector2));
            s_UnityEngine_Vector2_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector2>;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Vector3));
            s_UnityEngine_Vector3_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector3>;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Quaternion));
            s_UnityEngine_Quaternion_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Quaternion>;
        }

        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            s_UnityEngine_Vector2_Binding_Binder = null;
            s_UnityEngine_Vector3_Binding_Binder = null;
            s_UnityEngine_Quaternion_Binding_Binder = null;
        }
    }
}
