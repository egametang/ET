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
            System_String_Binding.Register(app);
            Model_Define_Binding.Register(app);
            System_Exception_Binding.Register(app);
            System_Collections_IDictionary_Binding.Register(app);
            System_Object_Binding.Register(app);
            Model_IdGenerater_Binding.Register(app);
            Model_Log_Binding.Register(app);
            System_Collections_Generic_HashSet_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_ILTypeInstance_Binding.Register(app);
            System_Type_Binding.Register(app);
            System_Reflection_MemberInfo_Binding.Register(app);
            System_Linq_Enumerable_Binding.Register(app);
            Model_EQueue_1_ILTypeInstance_Binding.Register(app);
            Model_IStart_Binding.Register(app);
            Model_DllHelper_Binding.Register(app);
            System_Activator_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_EQueue_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_UIType_ILTypeInstance_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            Model_UIFactoryAttribute_Binding.Register(app);
            Model_CanvasConfig_Binding.Register(app);
            Model_GameObjectHelper_Binding.Register(app);
            UnityEngine_Transform_Binding.Register(app);
            System_Collections_Generic_List_1_UIType_Binding.Register(app);
            Model_Disposer_Binding.Register(app);
            Model_Scene_Binding.Register(app);
            UnityEngine_Object_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding_ValueCollection_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding_ValueCollection_Binding_Enumerator_Binding.Register(app);
            System_IDisposable_Binding.Register(app);
            UnityEngine_Component_Binding.Register(app);
            Model_Game_Binding.Register(app);
            ReferenceCollector_Binding.Register(app);
            UnityEngine_UI_Button_Binding.Register(app);
            Model_ActionHelper_Binding.Register(app);
            Model_SessionComponent_Binding.Register(app);
            Model_Actor_Test_Binding.Register(app);
            Model_Session_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncVoidMethodBuilder_Binding.Register(app);
            Model_Actor_TestRequest_Binding.Register(app);
            System_Threading_Tasks_Task_1_Actor_TestResponse_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_Actor_TestResponse_Binding.Register(app);
            Model_MongoHelper_Binding.Register(app);
            Model_Actor_TransferRequest_Binding.Register(app);
            System_Threading_Tasks_Task_1_Actor_TransferResponse_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_Actor_TransferResponse_Binding.Register(app);
            Model_C2G_EnterMap_Binding.Register(app);
            System_Threading_Tasks_Task_1_G2C_EnterMap_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_G2C_EnterMap_Binding.Register(app);
            Model_Entity_Binding.Register(app);
            Model_ResourcesComponent_Binding.Register(app);
            UnityEngine_LayerMask_Binding.Register(app);
            Model_GlobalConfigComponent_Binding.Register(app);
            Model_GlobalProto_Binding.Register(app);
            Model_NetworkHelper_Binding.Register(app);
            Model_NetworkComponent_Binding.Register(app);
            UnityEngine_UI_InputField_Binding.Register(app);
            Model_C2R_Login_Binding.Register(app);
            Model_AResponse_Binding.Register(app);
            System_Int32_Binding.Register(app);
            Model_R2C_Login_Binding.Register(app);
            Model_C2G_LoginGate_Binding.Register(app);
            Model_G2C_LoginGate_Binding.Register(app);
            Model_EntityFactory_Binding.Register(app);
            Model_PlayerComponent_Binding.Register(app);
            Model_TimerComponent_Binding.Register(app);
            System_Threading_Tasks_Task_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_Binding.Register(app);
            UnityEngine_Debug_Binding.Register(app);
            UnityEngine_UI_Text_Binding.Register(app);
        }
    }
}
