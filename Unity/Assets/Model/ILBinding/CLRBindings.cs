using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {


        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            System_NotImplementedException_Binding.Register(app);
            System_String_Binding.Register(app);
            System_Text_StringBuilder_Binding.Register(app);
            System_Object_Binding.Register(app);
            System_Collections_IEnumerable_Binding.Register(app);
            System_Collections_IEnumerator_Binding.Register(app);
            System_IDisposable_Binding.Register(app);
            System_Type_Binding.Register(app);
            Google_Protobuf_ByteString_Binding.Register(app);
            ETModel_ByteHelper_Binding.Register(app);
            System_Array_Binding.Register(app);
            System_Reflection_MemberInfo_Binding.Register(app);
            System_Reflection_PropertyInfo_Binding.Register(app);
            System_Reflection_MethodBase_Binding.Register(app);
            UnityEngine_Debug_Binding.Register(app);
            System_Exception_Binding.Register(app);
            System_Collections_IDictionary_Binding.Register(app);
            LitJson_JsonMapper_Binding.Register(app);
            ETModel_Log_Binding.Register(app);
            ETModel_IdGenerater_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            UnityEngine_Transform_Binding.Register(app);
            UnityEngine_Object_Binding.Register(app);
            ETModel_MongoHelper_Binding.Register(app);
            ETModel_LayerNames_Binding.Register(app);
            ETModel_ComponentView_Binding.Register(app);
            System_Activator_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_ILTypeInstance_Binding_ValueCollection_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_ILTypeInstance_Binding_ValueCollection_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_HashSet_1_ILTypeInstance_Binding.Register(app);
            System_Linq_Enumerable_Binding.Register(app);
            System_Collections_Generic_HashSet_1_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_List_1_Object_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_List_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int64_ILTypeInstance_Binding.Register(app);
            ETModel_UnOrderMultiMap_2_Type_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Queue_1_Int64_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_List_1_Type_Binding.Register(app);
            ETModel_Game_Binding.Register(app);
            ETModel_Hotfix_Binding.Register(app);
            System_Collections_Generic_List_1_Type_Binding_Enumerator_Binding.Register(app);
            ETModel_EventAttribute_Binding.Register(app);
            ETModel_EventProxy_Binding.Register(app);
            ETModel_EventSystem_Binding.Register(app);
            System_Collections_Generic_Queue_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_Type_ILTypeInstance_Binding.Register(app);
            ETModel_Define_Binding.Register(app);
            ETModel_Entity_Binding.Register(app);
            ETModel_ResourcesComponent_Binding.Register(app);
            ETModel_ConfigAttribute_Binding.Register(app);
            ETModel_AppTypeHelper_Binding.Register(app);
            ETModel_GameObjectHelper_Binding.Register(app);
            UnityEngine_TextAsset_Binding.Register(app);
            ETModel_AsyncETVoidMethodBuilder_Binding.Register(app);
            ETModel_GlobalConfigComponent_Binding.Register(app);
            ETModel_GlobalProto_Binding.Register(app);
            ETModel_NetworkComponent_Binding.Register(app);
            ETModel_ETTask_1_Google_Protobuf_Adapt_IMessage_Binding_Adaptor_Binding.Register(app);
            ETModel_ETTask_1_Google_Protobuf_Adapt_IMessage_Binding_Adaptor_Binding_Awaiter_Binding.Register(app);
            ETModel_SessionComponent_Binding.Register(app);
            ETModel_ComponentFactory_Binding.Register(app);
            ETModel_PlayerComponent_Binding.Register(app);
            ETModel_ETTask_Binding.Register(app);
            ETModel_ETTask_Binding_Awaiter_Binding.Register(app);
            ETModel_SceneChangeComponent_Binding.Register(app);
            ETModel_C2G_EnterMap_Binding.Register(app);
            ETModel_Session_Binding.Register(app);
            ETModel_ETTask_1_IResponse_Binding.Register(app);
            ETModel_ETTask_1_IResponse_Binding_Awaiter_Binding.Register(app);
            ETModel_G2C_EnterMap_Binding.Register(app);
            ETModel_Player_Binding.Register(app);
            ETModel_M2C_CreateUnits_Binding.Register(app);
            Google_Protobuf_Collections_RepeatedField_1_UnitInfo_Binding.Register(app);
            System_Collections_Generic_IEnumerator_1_UnitInfo_Binding.Register(app);
            ETModel_UnitInfo_Binding.Register(app);
            ETModel_UnitComponent_Binding.Register(app);
            ETModel_UnitFactory_Binding.Register(app);
            UnityEngine_Vector3_Binding.Register(app);
            ETModel_Unit_Binding.Register(app);
            ETModel_M2C_PathfindingResult_Binding.Register(app);
            ETModel_AnimatorComponent_Binding.Register(app);
            ETModel_UnitPathComponent_Binding.Register(app);
            ETModel_ETVoid_Binding.Register(app);
            ETModel_GizmosDebug_Binding.Register(app);
            System_Collections_Generic_List_1_Vector3_Binding.Register(app);
            Google_Protobuf_Collections_RepeatedField_1_Single_Binding.Register(app);
            UnityEngine_LayerMask_Binding.Register(app);
            UnityEngine_Input_Binding.Register(app);
            UnityEngine_Camera_Binding.Register(app);
            UnityEngine_Physics_Binding.Register(app);
            UnityEngine_RaycastHit_Binding.Register(app);
            ETModel_Frame_ClickMap_Binding.Register(app);
            ReferenceCollector_Binding.Register(app);
            UnityEngine_UI_Button_Binding.Register(app);
            ETModel_ActionHelper_Binding.Register(app);
            ETModel_AssetBundleHelper_Binding.Register(app);
            UnityEngine_UI_InputField_Binding.Register(app);
            Google_Protobuf_ProtoPreconditions_Binding.Register(app);
            Google_Protobuf_CodedOutputStream_Binding.Register(app);
            Google_Protobuf_CodedInputStream_Binding.Register(app);
            Google_Protobuf_MessageParser_1_Google_Protobuf_Adapt_IMessage_Binding_Adaptor_Binding.Register(app);
            Google_Protobuf_Collections_RepeatedField_1_Google_Protobuf_Adapt_IMessage_Binding_Adaptor_Binding.Register(app);
            Google_Protobuf_Collections_RepeatedField_1_String_Binding.Register(app);
            Google_Protobuf_Collections_RepeatedField_1_Int32_Binding.Register(app);
            Google_Protobuf_Collections_RepeatedField_1_Int64_Binding.Register(app);
            Google_Protobuf_FieldCodec_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_UInt16_List_1_ILTypeInstance_Binding.Register(app);
            ETModel_OpcodeTypeComponent_Binding.Register(app);
            ETModel_MessageProxy_Binding.Register(app);
            ETModel_MessageDispatcherComponent_Binding.Register(app);
            ETModel_MessageInfo_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_Queue_1_Object_Binding.Register(app);
            System_Collections_Generic_Queue_1_Object_Binding.Register(app);
            ETModel_DoubleMap_2_UInt16_Type_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_UInt16_Object_Binding.Register(app);
            ETModel_MessageAttribute_Binding.Register(app);
            ETModel_SessionCallbackComponent_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Action_1_Google_Protobuf_Adapt_IMessage_Binding_Adaptor_Binding.Register(app);
            System_Action_1_Google_Protobuf_Adapt_IMessage_Binding_Adaptor_Binding.Register(app);
            ETModel_Component_Binding.Register(app);
            ETModel_IMessagePacker_Binding.Register(app);
            ETModel_OpcodeHelper_Binding.Register(app);
            ETModel_StringHelper_Binding.Register(app);
            ETModel_ETTaskCompletionSource_1_Google_Protobuf_Adapt_IMessage_Binding_Adaptor_Binding.Register(app);
            System_Threading_CancellationToken_Binding.Register(app);
            ETModel_ErrorCode_Binding.Register(app);
            ETModel_RpcException_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding_ValueCollection_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding_ValueCollection_Binding_Enumerator_Binding.Register(app);
            UnityEngine_Component_Binding.Register(app);
            UnityEngine_Canvas_Binding.Register(app);

            ILRuntime.CLR.TypeSystem.CLRType __clrType = null;
        }

        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
        }
    }
}
