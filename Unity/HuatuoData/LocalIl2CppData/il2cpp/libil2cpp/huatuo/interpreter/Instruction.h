#pragma once

#include "../CommonDef.h"

namespace huatuo
{
namespace interpreter
{

	enum class PrefixFlags
	{
		None = 0,
		Constrained = 0x1,
		No = 0x2,
		ReadOnly = 0x4,
		Tail = 0x8,
		Unaligned = 0x10,
		Volatile = 0x20,
	};

	extern uint16_t g_instructionSizes[];

	enum class HiOpcodeEnum : uint16_t
	{
		//!!!{{OPCODE
		InitLocals_n_2,
		InitLocals_n_4,
		LdlocVarVar,
		LdlocExpandVarVar_i1,
		LdlocExpandVarVar_u1,
		LdlocExpandVarVar_i2,
		LdlocExpandVarVar_u2,
		LdlocVarVarSize,
		LdlocVarAddress,
		LdcVarConst_1,
		LdcVarConst_2,
		LdcVarConst_4,
		LdcVarConst_8,
		LdnullVar,
		LdindVarVar_i1,
		LdindVarVar_u1,
		LdindVarVar_i2,
		LdindVarVar_u2,
		LdindVarVar_i4,
		LdindVarVar_u4,
		LdindVarVar_i8,
		LdindVarVar_f4,
		LdindVarVar_f8,
		StindVarVar_i1,
		StindVarVar_i2,
		StindVarVar_i4,
		StindVarVar_i8,
		StindVarVar_f4,
		StindVarVar_f8,
		LocalAllocVarVar_n_2,
		LocalAllocVarVar_n_4,
		InitblkVarVarVar,
		CpblkVarVar,
		MemoryBarrier,
		ConvertVarVar_i1_i1,
		ConvertVarVar_i1_u1,
		ConvertVarVar_i1_i2,
		ConvertVarVar_i1_u2,
		ConvertVarVar_i1_i4,
		ConvertVarVar_i1_u4,
		ConvertVarVar_i1_i8,
		ConvertVarVar_i1_u8,
		ConvertVarVar_i1_f4,
		ConvertVarVar_i1_f8,
		ConvertVarVar_u1_i1,
		ConvertVarVar_u1_u1,
		ConvertVarVar_u1_i2,
		ConvertVarVar_u1_u2,
		ConvertVarVar_u1_i4,
		ConvertVarVar_u1_u4,
		ConvertVarVar_u1_i8,
		ConvertVarVar_u1_u8,
		ConvertVarVar_u1_f4,
		ConvertVarVar_u1_f8,
		ConvertVarVar_i2_i1,
		ConvertVarVar_i2_u1,
		ConvertVarVar_i2_i2,
		ConvertVarVar_i2_u2,
		ConvertVarVar_i2_i4,
		ConvertVarVar_i2_u4,
		ConvertVarVar_i2_i8,
		ConvertVarVar_i2_u8,
		ConvertVarVar_i2_f4,
		ConvertVarVar_i2_f8,
		ConvertVarVar_u2_i1,
		ConvertVarVar_u2_u1,
		ConvertVarVar_u2_i2,
		ConvertVarVar_u2_u2,
		ConvertVarVar_u2_i4,
		ConvertVarVar_u2_u4,
		ConvertVarVar_u2_i8,
		ConvertVarVar_u2_u8,
		ConvertVarVar_u2_f4,
		ConvertVarVar_u2_f8,
		ConvertVarVar_i4_i1,
		ConvertVarVar_i4_u1,
		ConvertVarVar_i4_i2,
		ConvertVarVar_i4_u2,
		ConvertVarVar_i4_i4,
		ConvertVarVar_i4_u4,
		ConvertVarVar_i4_i8,
		ConvertVarVar_i4_u8,
		ConvertVarVar_i4_f4,
		ConvertVarVar_i4_f8,
		ConvertVarVar_u4_i1,
		ConvertVarVar_u4_u1,
		ConvertVarVar_u4_i2,
		ConvertVarVar_u4_u2,
		ConvertVarVar_u4_i4,
		ConvertVarVar_u4_u4,
		ConvertVarVar_u4_i8,
		ConvertVarVar_u4_u8,
		ConvertVarVar_u4_f4,
		ConvertVarVar_u4_f8,
		ConvertVarVar_i8_i1,
		ConvertVarVar_i8_u1,
		ConvertVarVar_i8_i2,
		ConvertVarVar_i8_u2,
		ConvertVarVar_i8_i4,
		ConvertVarVar_i8_u4,
		ConvertVarVar_i8_i8,
		ConvertVarVar_i8_u8,
		ConvertVarVar_i8_f4,
		ConvertVarVar_i8_f8,
		ConvertVarVar_u8_i1,
		ConvertVarVar_u8_u1,
		ConvertVarVar_u8_i2,
		ConvertVarVar_u8_u2,
		ConvertVarVar_u8_i4,
		ConvertVarVar_u8_u4,
		ConvertVarVar_u8_i8,
		ConvertVarVar_u8_u8,
		ConvertVarVar_u8_f4,
		ConvertVarVar_u8_f8,
		ConvertVarVar_f4_i1,
		ConvertVarVar_f4_u1,
		ConvertVarVar_f4_i2,
		ConvertVarVar_f4_u2,
		ConvertVarVar_f4_i4,
		ConvertVarVar_f4_u4,
		ConvertVarVar_f4_i8,
		ConvertVarVar_f4_u8,
		ConvertVarVar_f4_f4,
		ConvertVarVar_f4_f8,
		ConvertVarVar_f8_i1,
		ConvertVarVar_f8_u1,
		ConvertVarVar_f8_i2,
		ConvertVarVar_f8_u2,
		ConvertVarVar_f8_i4,
		ConvertVarVar_f8_u4,
		ConvertVarVar_f8_i8,
		ConvertVarVar_f8_u8,
		ConvertVarVar_f8_f4,
		ConvertVarVar_f8_f8,
		ConvertOverflowVarVar_i1_i1,
		ConvertOverflowVarVar_i1_u1,
		ConvertOverflowVarVar_i1_i2,
		ConvertOverflowVarVar_i1_u2,
		ConvertOverflowVarVar_i1_i4,
		ConvertOverflowVarVar_i1_u4,
		ConvertOverflowVarVar_i1_i8,
		ConvertOverflowVarVar_i1_u8,
		ConvertOverflowVarVar_i1_f4,
		ConvertOverflowVarVar_i1_f8,
		ConvertOverflowVarVar_u1_i1,
		ConvertOverflowVarVar_u1_u1,
		ConvertOverflowVarVar_u1_i2,
		ConvertOverflowVarVar_u1_u2,
		ConvertOverflowVarVar_u1_i4,
		ConvertOverflowVarVar_u1_u4,
		ConvertOverflowVarVar_u1_i8,
		ConvertOverflowVarVar_u1_u8,
		ConvertOverflowVarVar_u1_f4,
		ConvertOverflowVarVar_u1_f8,
		ConvertOverflowVarVar_i2_i1,
		ConvertOverflowVarVar_i2_u1,
		ConvertOverflowVarVar_i2_i2,
		ConvertOverflowVarVar_i2_u2,
		ConvertOverflowVarVar_i2_i4,
		ConvertOverflowVarVar_i2_u4,
		ConvertOverflowVarVar_i2_i8,
		ConvertOverflowVarVar_i2_u8,
		ConvertOverflowVarVar_i2_f4,
		ConvertOverflowVarVar_i2_f8,
		ConvertOverflowVarVar_u2_i1,
		ConvertOverflowVarVar_u2_u1,
		ConvertOverflowVarVar_u2_i2,
		ConvertOverflowVarVar_u2_u2,
		ConvertOverflowVarVar_u2_i4,
		ConvertOverflowVarVar_u2_u4,
		ConvertOverflowVarVar_u2_i8,
		ConvertOverflowVarVar_u2_u8,
		ConvertOverflowVarVar_u2_f4,
		ConvertOverflowVarVar_u2_f8,
		ConvertOverflowVarVar_i4_i1,
		ConvertOverflowVarVar_i4_u1,
		ConvertOverflowVarVar_i4_i2,
		ConvertOverflowVarVar_i4_u2,
		ConvertOverflowVarVar_i4_i4,
		ConvertOverflowVarVar_i4_u4,
		ConvertOverflowVarVar_i4_i8,
		ConvertOverflowVarVar_i4_u8,
		ConvertOverflowVarVar_i4_f4,
		ConvertOverflowVarVar_i4_f8,
		ConvertOverflowVarVar_u4_i1,
		ConvertOverflowVarVar_u4_u1,
		ConvertOverflowVarVar_u4_i2,
		ConvertOverflowVarVar_u4_u2,
		ConvertOverflowVarVar_u4_i4,
		ConvertOverflowVarVar_u4_u4,
		ConvertOverflowVarVar_u4_i8,
		ConvertOverflowVarVar_u4_u8,
		ConvertOverflowVarVar_u4_f4,
		ConvertOverflowVarVar_u4_f8,
		ConvertOverflowVarVar_i8_i1,
		ConvertOverflowVarVar_i8_u1,
		ConvertOverflowVarVar_i8_i2,
		ConvertOverflowVarVar_i8_u2,
		ConvertOverflowVarVar_i8_i4,
		ConvertOverflowVarVar_i8_u4,
		ConvertOverflowVarVar_i8_i8,
		ConvertOverflowVarVar_i8_u8,
		ConvertOverflowVarVar_i8_f4,
		ConvertOverflowVarVar_i8_f8,
		ConvertOverflowVarVar_u8_i1,
		ConvertOverflowVarVar_u8_u1,
		ConvertOverflowVarVar_u8_i2,
		ConvertOverflowVarVar_u8_u2,
		ConvertOverflowVarVar_u8_i4,
		ConvertOverflowVarVar_u8_u4,
		ConvertOverflowVarVar_u8_i8,
		ConvertOverflowVarVar_u8_u8,
		ConvertOverflowVarVar_u8_f4,
		ConvertOverflowVarVar_u8_f8,
		ConvertOverflowVarVar_f4_i1,
		ConvertOverflowVarVar_f4_u1,
		ConvertOverflowVarVar_f4_i2,
		ConvertOverflowVarVar_f4_u2,
		ConvertOverflowVarVar_f4_i4,
		ConvertOverflowVarVar_f4_u4,
		ConvertOverflowVarVar_f4_i8,
		ConvertOverflowVarVar_f4_u8,
		ConvertOverflowVarVar_f4_f4,
		ConvertOverflowVarVar_f4_f8,
		ConvertOverflowVarVar_f8_i1,
		ConvertOverflowVarVar_f8_u1,
		ConvertOverflowVarVar_f8_i2,
		ConvertOverflowVarVar_f8_u2,
		ConvertOverflowVarVar_f8_i4,
		ConvertOverflowVarVar_f8_u4,
		ConvertOverflowVarVar_f8_i8,
		ConvertOverflowVarVar_f8_u8,
		ConvertOverflowVarVar_f8_f4,
		ConvertOverflowVarVar_f8_f8,
		BinOpVarVarVar_Add_i4,
		BinOpVarVarVar_Sub_i4,
		BinOpVarVarVar_Mul_i4,
		BinOpVarVarVar_MulUn_i4,
		BinOpVarVarVar_Div_i4,
		BinOpVarVarVar_DivUn_i4,
		BinOpVarVarVar_Rem_i4,
		BinOpVarVarVar_RemUn_i4,
		BinOpVarVarVar_And_i4,
		BinOpVarVarVar_Or_i4,
		BinOpVarVarVar_Xor_i4,
		BinOpVarVarVar_Add_i8,
		BinOpVarVarVar_Sub_i8,
		BinOpVarVarVar_Mul_i8,
		BinOpVarVarVar_MulUn_i8,
		BinOpVarVarVar_Div_i8,
		BinOpVarVarVar_DivUn_i8,
		BinOpVarVarVar_Rem_i8,
		BinOpVarVarVar_RemUn_i8,
		BinOpVarVarVar_And_i8,
		BinOpVarVarVar_Or_i8,
		BinOpVarVarVar_Xor_i8,
		BinOpVarVarVar_Add_f4,
		BinOpVarVarVar_Sub_f4,
		BinOpVarVarVar_Mul_f4,
		BinOpVarVarVar_Div_f4,
		BinOpVarVarVar_Rem_f4,
		BinOpVarVarVar_Add_f8,
		BinOpVarVarVar_Sub_f8,
		BinOpVarVarVar_Mul_f8,
		BinOpVarVarVar_Div_f8,
		BinOpVarVarVar_Rem_f8,
		BinOpOverflowVarVarVar_Add_i4,
		BinOpOverflowVarVarVar_Sub_i4,
		BinOpOverflowVarVarVar_Mul_i4,
		BinOpOverflowVarVarVar_Add_i8,
		BinOpOverflowVarVarVar_Sub_i8,
		BinOpOverflowVarVarVar_Mul_i8,
		BinOpOverflowVarVarVar_Add_u4,
		BinOpOverflowVarVarVar_Sub_u4,
		BinOpOverflowVarVarVar_Mul_u4,
		BinOpOverflowVarVarVar_Add_u8,
		BinOpOverflowVarVarVar_Sub_u8,
		BinOpOverflowVarVarVar_Mul_u8,
		BitShiftBinOpVarVarVar_Shl_i4_i4,
		BitShiftBinOpVarVarVar_Shr_i4_i4,
		BitShiftBinOpVarVarVar_ShrUn_i4_i4,
		BitShiftBinOpVarVarVar_Shl_i4_i8,
		BitShiftBinOpVarVarVar_Shr_i4_i8,
		BitShiftBinOpVarVarVar_ShrUn_i4_i8,
		BitShiftBinOpVarVarVar_Shl_i8_i4,
		BitShiftBinOpVarVarVar_Shr_i8_i4,
		BitShiftBinOpVarVarVar_ShrUn_i8_i4,
		BitShiftBinOpVarVarVar_Shl_i8_i8,
		BitShiftBinOpVarVarVar_Shr_i8_i8,
		BitShiftBinOpVarVarVar_ShrUn_i8_i8,
		UnaryOpVarVar_Neg_i4,
		UnaryOpVarVar_Not_i4,
		UnaryOpVarVar_Neg_i8,
		UnaryOpVarVar_Not_i8,
		UnaryOpVarVar_Neg_f4,
		UnaryOpVarVar_Neg_f8,
		CheckFiniteVar_f4,
		CheckFiniteVar_f8,
		CompOpVarVarVar_Ceq_i4,
		CompOpVarVarVar_Ceq_i8,
		CompOpVarVarVar_Ceq_f4,
		CompOpVarVarVar_Ceq_f8,
		CompOpVarVarVar_Cgt_i4,
		CompOpVarVarVar_Cgt_i8,
		CompOpVarVarVar_Cgt_f4,
		CompOpVarVarVar_Cgt_f8,
		CompOpVarVarVar_CgtUn_i4,
		CompOpVarVarVar_CgtUn_i8,
		CompOpVarVarVar_CgtUn_f4,
		CompOpVarVarVar_CgtUn_f8,
		CompOpVarVarVar_Clt_i4,
		CompOpVarVarVar_Clt_i8,
		CompOpVarVarVar_Clt_f4,
		CompOpVarVarVar_Clt_f8,
		CompOpVarVarVar_CltUn_i4,
		CompOpVarVarVar_CltUn_i8,
		CompOpVarVarVar_CltUn_f4,
		CompOpVarVarVar_CltUn_f8,
		BranchUncondition_4,
		BranchTrueVar_i4,
		BranchTrueVar_i8,
		BranchFalseVar_i4,
		BranchFalseVar_i8,
		BranchVarVar_Ceq_i4,
		BranchVarVar_Ceq_i8,
		BranchVarVar_Ceq_f4,
		BranchVarVar_Ceq_f8,
		BranchVarVar_CneUn_i4,
		BranchVarVar_CneUn_i8,
		BranchVarVar_CneUn_f4,
		BranchVarVar_CneUn_f8,
		BranchVarVar_Cgt_i4,
		BranchVarVar_Cgt_i8,
		BranchVarVar_Cgt_f4,
		BranchVarVar_Cgt_f8,
		BranchVarVar_CgtUn_i4,
		BranchVarVar_CgtUn_i8,
		BranchVarVar_CgtUn_f4,
		BranchVarVar_CgtUn_f8,
		BranchVarVar_Cge_i4,
		BranchVarVar_Cge_i8,
		BranchVarVar_Cge_f4,
		BranchVarVar_Cge_f8,
		BranchVarVar_CgeUn_i4,
		BranchVarVar_CgeUn_i8,
		BranchVarVar_CgeUn_f4,
		BranchVarVar_CgeUn_f8,
		BranchVarVar_Clt_i4,
		BranchVarVar_Clt_i8,
		BranchVarVar_Clt_f4,
		BranchVarVar_Clt_f8,
		BranchVarVar_CltUn_i4,
		BranchVarVar_CltUn_i8,
		BranchVarVar_CltUn_f4,
		BranchVarVar_CltUn_f8,
		BranchVarVar_Cle_i4,
		BranchVarVar_Cle_i8,
		BranchVarVar_Cle_f4,
		BranchVarVar_Cle_f8,
		BranchVarVar_CleUn_i4,
		BranchVarVar_CleUn_i8,
		BranchVarVar_CleUn_f4,
		BranchVarVar_CleUn_f8,
		BranchJump,
		BranchSwitch,
		NewClassVar,
		NewClassVar_Ctor_0,
		NewClassVar_NotCtor,
		NewValueTypeVar,
		NewClassInterpVar,
		NewClassInterpVar_Ctor_0,
		NewValueTypeInterpVar,
		AdjustValueTypeRefVar,
		BoxRefVarVar,
		LdvirftnVarVar,
		RetVar_ret_1,
		RetVar_ret_2,
		RetVar_ret_4,
		RetVar_ret_8,
		RetVar_ret_12,
		RetVar_ret_16,
		RetVar_ret_20,
		RetVar_ret_24,
		RetVar_ret_28,
		RetVar_ret_32,
		RetVar_ret_n,
		RetVar_void,
		CallNative_void,
		CallNative_ret,
		CallNative_ret_expand,
		CallInterp_void,
		CallInterp_ret,
		CallVirtual_void,
		CallVirtual_ret,
		CallVirtual_ret_expand,
		CallInterpVirtual_void,
		CallInterpVirtual_ret,
		CallInd_void,
		CallInd_ret,
		CallInd_ret_expand,
		CallDelegate_void,
		CallDelegate_ret,
		CallDelegate_ret_expand,
		NewDelegate,
		BoxVarVar,
		UnBoxVarVar,
		UnBoxAnyVarVar,
		CastclassVar,
		IsInstVar,
		LdtokenVar,
		MakeRefVarVar,
		RefAnyTypeVarVar,
		RefAnyValueVarVar,
		CpobjVarVar_1,
		CpobjVarVar_2,
		CpobjVarVar_4,
		CpobjVarVar_8,
		CpobjVarVar_12,
		CpobjVarVar_16,
		CpobjVarVar_20,
		CpobjVarVar_24,
		CpobjVarVar_28,
		CpobjVarVar_32,
		CpobjVarVar_n_2,
		CpobjVarVar_n_4,
		LdobjVarVar_1,
		LdobjVarVar_2,
		LdobjVarVar_4,
		LdobjVarVar_8,
		LdobjVarVar_12,
		LdobjVarVar_16,
		LdobjVarVar_20,
		LdobjVarVar_24,
		LdobjVarVar_28,
		LdobjVarVar_32,
		LdobjVarVar_n_4,
		StobjVarVar_1,
		StobjVarVar_2,
		StobjVarVar_4,
		StobjVarVar_8,
		StobjVarVar_12,
		StobjVarVar_16,
		StobjVarVar_20,
		StobjVarVar_24,
		StobjVarVar_28,
		StobjVarVar_32,
		StobjVarVar_n_4,
		InitobjVar_1,
		InitobjVar_2,
		InitobjVar_4,
		InitobjVar_8,
		InitobjVar_12,
		InitobjVar_16,
		InitobjVar_20,
		InitobjVar_24,
		InitobjVar_28,
		InitobjVar_32,
		InitobjVar_n_2,
		InitobjVar_n_4,
		LdstrVar,
		LdfldVarVar_i1,
		LdfldVarVar_u1,
		LdfldVarVar_i2,
		LdfldVarVar_u2,
		LdfldVarVar_i4,
		LdfldVarVar_u4,
		LdfldVarVar_i8,
		LdfldVarVar_u8,
		LdfldVarVar_size_8,
		LdfldVarVar_size_12,
		LdfldVarVar_size_16,
		LdfldVarVar_size_20,
		LdfldVarVar_size_24,
		LdfldVarVar_size_28,
		LdfldVarVar_size_32,
		LdfldVarVar_n_2,
		LdfldVarVar_n_4,
		LdfldValueTypeVarVar_i1,
		LdfldValueTypeVarVar_u1,
		LdfldValueTypeVarVar_i2,
		LdfldValueTypeVarVar_u2,
		LdfldValueTypeVarVar_i4,
		LdfldValueTypeVarVar_u4,
		LdfldValueTypeVarVar_i8,
		LdfldValueTypeVarVar_u8,
		LdfldValueTypeVarVar_size_8,
		LdfldValueTypeVarVar_size_12,
		LdfldValueTypeVarVar_size_16,
		LdfldValueTypeVarVar_size_20,
		LdfldValueTypeVarVar_size_24,
		LdfldValueTypeVarVar_size_28,
		LdfldValueTypeVarVar_size_32,
		LdfldValueTypeVarVar_n_2,
		LdfldValueTypeVarVar_n_4,
		LdfldaVarVar,
		StfldVarVar_i1,
		StfldVarVar_u1,
		StfldVarVar_i2,
		StfldVarVar_u2,
		StfldVarVar_i4,
		StfldVarVar_u4,
		StfldVarVar_i8,
		StfldVarVar_u8,
		StfldVarVar_size_8,
		StfldVarVar_size_12,
		StfldVarVar_size_16,
		StfldVarVar_size_20,
		StfldVarVar_size_24,
		StfldVarVar_size_28,
		StfldVarVar_size_32,
		StfldVarVar_n_2,
		StfldVarVar_n_4,
		LdsfldVarVar_i1,
		LdsfldVarVar_u1,
		LdsfldVarVar_i2,
		LdsfldVarVar_u2,
		LdsfldVarVar_i4,
		LdsfldVarVar_u4,
		LdsfldVarVar_i8,
		LdsfldVarVar_u8,
		LdsfldVarVar_size_8,
		LdsfldVarVar_size_12,
		LdsfldVarVar_size_16,
		LdsfldVarVar_size_20,
		LdsfldVarVar_size_24,
		LdsfldVarVar_size_28,
		LdsfldVarVar_size_32,
		LdsfldVarVar_n_2,
		LdsfldVarVar_n_4,
		StsfldVarVar_i1,
		StsfldVarVar_u1,
		StsfldVarVar_i2,
		StsfldVarVar_u2,
		StsfldVarVar_i4,
		StsfldVarVar_u4,
		StsfldVarVar_i8,
		StsfldVarVar_u8,
		StsfldVarVar_size_8,
		StsfldVarVar_size_12,
		StsfldVarVar_size_16,
		StsfldVarVar_size_20,
		StsfldVarVar_size_24,
		StsfldVarVar_size_28,
		StsfldVarVar_size_32,
		StsfldVarVar_n_2,
		StsfldVarVar_n_4,
		LdsfldaVarVar,
		LdsfldaFromFieldDataVarVar,
		LdthreadlocalaVarVar,
		LdthreadlocalVarVar_i1,
		LdthreadlocalVarVar_u1,
		LdthreadlocalVarVar_i2,
		LdthreadlocalVarVar_u2,
		LdthreadlocalVarVar_i4,
		LdthreadlocalVarVar_u4,
		LdthreadlocalVarVar_i8,
		LdthreadlocalVarVar_u8,
		LdthreadlocalVarVar_size_8,
		LdthreadlocalVarVar_size_12,
		LdthreadlocalVarVar_size_16,
		LdthreadlocalVarVar_size_20,
		LdthreadlocalVarVar_size_24,
		LdthreadlocalVarVar_size_28,
		LdthreadlocalVarVar_size_32,
		LdthreadlocalVarVar_n_2,
		LdthreadlocalVarVar_n_4,
		StthreadlocalVarVar_i1,
		StthreadlocalVarVar_u1,
		StthreadlocalVarVar_i2,
		StthreadlocalVarVar_u2,
		StthreadlocalVarVar_i4,
		StthreadlocalVarVar_u4,
		StthreadlocalVarVar_i8,
		StthreadlocalVarVar_u8,
		StthreadlocalVarVar_size_8,
		StthreadlocalVarVar_size_12,
		StthreadlocalVarVar_size_16,
		StthreadlocalVarVar_size_20,
		StthreadlocalVarVar_size_24,
		StthreadlocalVarVar_size_28,
		StthreadlocalVarVar_size_32,
		StthreadlocalVarVar_n_2,
		StthreadlocalVarVar_n_4,
		NewArrVarVar_4,
		NewArrVarVar_8,
		GetArrayLengthVarVar_4,
		GetArrayLengthVarVar_8,
		GetArrayElementAddressAddrVarVar_i4,
		GetArrayElementAddressAddrVarVar_i8,
		GetArrayElementAddressCheckAddrVarVar_i4,
		GetArrayElementAddressCheckAddrVarVar_i8,
		GetArrayElementVarVar_i1_4,
		GetArrayElementVarVar_u1_4,
		GetArrayElementVarVar_i2_4,
		GetArrayElementVarVar_u2_4,
		GetArrayElementVarVar_i4_4,
		GetArrayElementVarVar_u4_4,
		GetArrayElementVarVar_i8_4,
		GetArrayElementVarVar_u8_4,
		GetArrayElementVarVar_size_12_4,
		GetArrayElementVarVar_size_16_4,
		GetArrayElementVarVar_n_4,
		GetArrayElementVarVar_i1_8,
		GetArrayElementVarVar_u1_8,
		GetArrayElementVarVar_i2_8,
		GetArrayElementVarVar_u2_8,
		GetArrayElementVarVar_i4_8,
		GetArrayElementVarVar_u4_8,
		GetArrayElementVarVar_i8_8,
		GetArrayElementVarVar_u8_8,
		GetArrayElementVarVar_size_12_8,
		GetArrayElementVarVar_size_16_8,
		GetArrayElementVarVar_n_8,
		SetArrayElementVarVar_i1_4,
		SetArrayElementVarVar_u1_4,
		SetArrayElementVarVar_i2_4,
		SetArrayElementVarVar_u2_4,
		SetArrayElementVarVar_i4_4,
		SetArrayElementVarVar_u4_4,
		SetArrayElementVarVar_i8_4,
		SetArrayElementVarVar_u8_4,
		SetArrayElementVarVar_ref_4,
		SetArrayElementVarVar_size_12_4,
		SetArrayElementVarVar_size_16_4,
		SetArrayElementVarVar_n_4,
		SetArrayElementVarVar_i1_8,
		SetArrayElementVarVar_u1_8,
		SetArrayElementVarVar_i2_8,
		SetArrayElementVarVar_u2_8,
		SetArrayElementVarVar_i4_8,
		SetArrayElementVarVar_u4_8,
		SetArrayElementVarVar_i8_8,
		SetArrayElementVarVar_u8_8,
		SetArrayElementVarVar_ref_8,
		SetArrayElementVarVar_size_12_8,
		SetArrayElementVarVar_size_16_8,
		SetArrayElementVarVar_n_8,
		NewMdArrVarVar_length,
		NewMdArrVarVar_length_bound,
		GetMdArrElementVarVar_i1,
		GetMdArrElementVarVar_u1,
		GetMdArrElementVarVar_i2,
		GetMdArrElementVarVar_u2,
		GetMdArrElementVarVar_i4,
		GetMdArrElementVarVar_u4,
		GetMdArrElementVarVar_i8,
		GetMdArrElementVarVar_u8,
		GetMdArrElementVarVar_size,
		GetMdArrElementAddressVarVar,
		SetMdArrElementVarVar,
		ThrowEx,
		RethrowEx,
		LeaveEx,
		EndFilterEx,
		EndFinallyEx,
		NullableNewVarVar,
		NullableCtorVarVar,
		NullableHasValueVar,
		NullableGetValueOrDefaultVarVar,
		NullableGetValueOrDefaultVarVar_1,
		NullableGetValueVarVar,
		InterlockedCompareExchangeVarVarVarVar_i4,
		InterlockedCompareExchangeVarVarVarVar_i8,
		InterlockedCompareExchangeVarVarVarVar_pointer,
		InterlockedExchangeVarVarVar_i4,
		InterlockedExchangeVarVarVar_i8,
		InterlockedExchangeVarVarVar_pointer,
		NewSystemObjectVar,
		NewVector2,
		NewVector3_2,
		NewVector3_3,
		NewVector4_2,
		NewVector4_3,
		NewVector4_4,

		//!!!}}OPCODE
	};

	struct IRCommon
	{
		HiOpcodeEnum type;
	};

#pragma region instruction
#pragma pack(push, 1)
	//!!!{{INST

	struct IRInitLocals_n_2 : IRCommon
	{
		uint16_t size;
	};


	struct IRInitLocals_n_4 : IRCommon
	{
		uint32_t size;
	};


	struct IRLdlocVarVar : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdlocExpandVarVar_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdlocExpandVarVar_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdlocExpandVarVar_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdlocExpandVarVar_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdlocVarVarSize : IRCommon
	{
		uint16_t dst;
		uint16_t src;
		uint16_t size;
	};


	struct IRLdlocVarAddress : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdcVarConst_1 : IRCommon
	{
		uint16_t dst;
		uint8_t src;
		uint8_t __pad__;
	};


	struct IRLdcVarConst_2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdcVarConst_4 : IRCommon
	{
		uint16_t dst;
		uint32_t src;
	};


	struct IRLdcVarConst_8 : IRCommon
	{
		uint16_t dst;
		uint64_t src;
	};


	struct IRLdnullVar : IRCommon
	{
		uint16_t dst;
	};


	struct IRLdindVarVar_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdindVarVar_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdindVarVar_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdindVarVar_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdindVarVar_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdindVarVar_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdindVarVar_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdindVarVar_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdindVarVar_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStindVarVar_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStindVarVar_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStindVarVar_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStindVarVar_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStindVarVar_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStindVarVar_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLocalAllocVarVar_n_2 : IRCommon
	{
		uint16_t dst;
		uint16_t size;
	};


	struct IRLocalAllocVarVar_n_4 : IRCommon
	{
		uint16_t dst;
		uint16_t size;
	};


	struct IRInitblkVarVarVar : IRCommon
	{
		uint16_t addr;
		uint16_t value;
		uint16_t size;
	};


	struct IRCpblkVarVar : IRCommon
	{
		uint16_t dst;
		uint16_t src;
		uint16_t size;
	};


	struct IRMemoryBarrier : IRCommon
	{

	};


	struct IRConvertVarVar_i1_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i1_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i1_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i1_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i1_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i1_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i1_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i1_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i1_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i1_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u1_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u1_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u1_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u1_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u1_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u1_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u1_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u1_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u1_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u1_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i2_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i2_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i2_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i2_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i2_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i2_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i2_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i2_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i2_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i2_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u2_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u2_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u2_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u2_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u2_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u2_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u2_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u2_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u2_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u2_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i4_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i4_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i4_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i4_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i4_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i4_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i4_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i4_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i4_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i4_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u4_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u4_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u4_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u4_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u4_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u4_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u4_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u4_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u4_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u4_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i8_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i8_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i8_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i8_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i8_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i8_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i8_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i8_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i8_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_i8_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u8_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u8_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u8_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u8_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u8_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u8_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u8_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u8_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u8_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_u8_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f4_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f4_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f4_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f4_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f4_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f4_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f4_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f4_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f4_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f4_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f8_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f8_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f8_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f8_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f8_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f8_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f8_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f8_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f8_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertVarVar_f8_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i1_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i1_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i1_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i1_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i1_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i1_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i1_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i1_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i1_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i1_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u1_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u1_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u1_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u1_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u1_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u1_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u1_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u1_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u1_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u1_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i2_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i2_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i2_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i2_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i2_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i2_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i2_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i2_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i2_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i2_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u2_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u2_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u2_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u2_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u2_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u2_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u2_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u2_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u2_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u2_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i4_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i4_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i4_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i4_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i4_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i4_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i4_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i4_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i4_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i4_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u4_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u4_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u4_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u4_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u4_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u4_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u4_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u4_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u4_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u4_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i8_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i8_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i8_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i8_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i8_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i8_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i8_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i8_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i8_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_i8_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u8_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u8_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u8_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u8_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u8_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u8_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u8_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u8_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u8_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_u8_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f4_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f4_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f4_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f4_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f4_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f4_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f4_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f4_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f4_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f4_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f8_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f8_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f8_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f8_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f8_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f8_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f8_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f8_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f8_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRConvertOverflowVarVar_f8_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRBinOpVarVarVar_Add_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Sub_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Mul_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_MulUn_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Div_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_DivUn_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Rem_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_RemUn_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_And_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Or_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Xor_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Add_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Sub_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Mul_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_MulUn_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Div_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_DivUn_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Rem_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_RemUn_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_And_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Or_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Xor_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Add_f4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Sub_f4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Mul_f4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Div_f4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Rem_f4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Add_f8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Sub_f8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Mul_f8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Div_f8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpVarVarVar_Rem_f8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpOverflowVarVarVar_Add_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpOverflowVarVarVar_Sub_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpOverflowVarVarVar_Mul_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpOverflowVarVarVar_Add_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpOverflowVarVarVar_Sub_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpOverflowVarVarVar_Mul_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpOverflowVarVarVar_Add_u4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpOverflowVarVarVar_Sub_u4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpOverflowVarVarVar_Mul_u4 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpOverflowVarVarVar_Add_u8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpOverflowVarVarVar_Sub_u8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBinOpOverflowVarVarVar_Mul_u8 : IRCommon
	{
		uint16_t ret;
		uint16_t op1;
		uint16_t op2;
	};


	struct IRBitShiftBinOpVarVarVar_Shl_i4_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t value;
		uint16_t shiftAmount;
	};


	struct IRBitShiftBinOpVarVarVar_Shr_i4_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t value;
		uint16_t shiftAmount;
	};


	struct IRBitShiftBinOpVarVarVar_ShrUn_i4_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t value;
		uint16_t shiftAmount;
	};


	struct IRBitShiftBinOpVarVarVar_Shl_i4_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t value;
		uint16_t shiftAmount;
	};


	struct IRBitShiftBinOpVarVarVar_Shr_i4_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t value;
		uint16_t shiftAmount;
	};


	struct IRBitShiftBinOpVarVarVar_ShrUn_i4_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t value;
		uint16_t shiftAmount;
	};


	struct IRBitShiftBinOpVarVarVar_Shl_i8_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t value;
		uint16_t shiftAmount;
	};


	struct IRBitShiftBinOpVarVarVar_Shr_i8_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t value;
		uint16_t shiftAmount;
	};


	struct IRBitShiftBinOpVarVarVar_ShrUn_i8_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t value;
		uint16_t shiftAmount;
	};


	struct IRBitShiftBinOpVarVarVar_Shl_i8_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t value;
		uint16_t shiftAmount;
	};


	struct IRBitShiftBinOpVarVarVar_Shr_i8_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t value;
		uint16_t shiftAmount;
	};


	struct IRBitShiftBinOpVarVarVar_ShrUn_i8_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t value;
		uint16_t shiftAmount;
	};


	struct IRUnaryOpVarVar_Neg_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRUnaryOpVarVar_Not_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRUnaryOpVarVar_Neg_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRUnaryOpVarVar_Not_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRUnaryOpVarVar_Neg_f4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRUnaryOpVarVar_Neg_f8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRCheckFiniteVar_f4 : IRCommon
	{
		uint16_t src;
	};


	struct IRCheckFiniteVar_f8 : IRCommon
	{
		uint16_t src;
	};


	struct IRCompOpVarVarVar_Ceq_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_Ceq_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_Ceq_f4 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_Ceq_f8 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_Cgt_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_Cgt_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_Cgt_f4 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_Cgt_f8 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_CgtUn_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_CgtUn_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_CgtUn_f4 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_CgtUn_f8 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_Clt_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_Clt_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_Clt_f4 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_Clt_f8 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_CltUn_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_CltUn_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_CltUn_f4 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRCompOpVarVarVar_CltUn_f8 : IRCommon
	{
		uint16_t ret;
		uint16_t c1;
		uint16_t c2;
	};


	struct IRBranchUncondition_4 : IRCommon
	{
		int32_t offset;
	};


	struct IRBranchTrueVar_i4 : IRCommon
	{
		uint16_t op;
		int32_t offset;
	};


	struct IRBranchTrueVar_i8 : IRCommon
	{
		uint16_t op;
		int32_t offset;
	};


	struct IRBranchFalseVar_i4 : IRCommon
	{
		uint16_t op;
		int32_t offset;
	};


	struct IRBranchFalseVar_i8 : IRCommon
	{
		uint16_t op;
		int32_t offset;
	};


	struct IRBranchVarVar_Ceq_i4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Ceq_i8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Ceq_f4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Ceq_f8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CneUn_i4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CneUn_i8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CneUn_f4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CneUn_f8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Cgt_i4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Cgt_i8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Cgt_f4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Cgt_f8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CgtUn_i4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CgtUn_i8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CgtUn_f4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CgtUn_f8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Cge_i4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Cge_i8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Cge_f4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Cge_f8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CgeUn_i4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CgeUn_i8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CgeUn_f4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CgeUn_f8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Clt_i4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Clt_i8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Clt_f4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Clt_f8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CltUn_i4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CltUn_i8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CltUn_f4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CltUn_f8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Cle_i4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Cle_i8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Cle_f4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_Cle_f8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CleUn_i4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CleUn_i8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CleUn_f4 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchVarVar_CleUn_f8 : IRCommon
	{
		uint16_t op1;
		uint16_t op2;
		int32_t offset;
	};


	struct IRBranchJump : IRCommon
	{
		uint32_t token;
	};


	struct IRBranchSwitch : IRCommon
	{
		uint16_t value;
		uint32_t caseNum;
		uint32_t caseOffsets;
	};


	struct IRNewClassVar : IRCommon
	{
		uint16_t obj;
		void* managed2NativeMethod;
		MethodInfo* method;
		uint32_t argIdxs;
	};


	struct IRNewClassVar_Ctor_0 : IRCommon
	{
		uint16_t obj;
		MethodInfo* method;
	};


	struct IRNewClassVar_NotCtor : IRCommon
	{
		uint16_t obj;
		Il2CppClass* klass;
	};


	struct IRNewValueTypeVar : IRCommon
	{
		uint16_t obj;
		void* managed2NativeMethod;
		MethodInfo* method;
		uint32_t argIdxs;
	};


	struct IRNewClassInterpVar : IRCommon
	{
		uint16_t obj;
		MethodInfo* method;
		uint16_t argBase;
		uint16_t argStackObjectNum;
		uint16_t ctorFrameBase;
	};


	struct IRNewClassInterpVar_Ctor_0 : IRCommon
	{
		uint16_t obj;
		MethodInfo* method;
		uint16_t ctorFrameBase;
	};


	struct IRNewValueTypeInterpVar : IRCommon
	{
		uint16_t obj;
		MethodInfo* method;
		uint16_t argBase;
		uint16_t argStackObjectNum;
		uint16_t ctorFrameBase;
	};


	struct IRAdjustValueTypeRefVar : IRCommon
	{
		uint16_t data;
	};


	struct IRBoxRefVarVar : IRCommon
	{
		uint16_t dst;
		uint16_t src;
		Il2CppClass* klass;
	};


	struct IRLdvirftnVarVar : IRCommon
	{
		uint16_t resultMethod;
		uint16_t obj;
		MethodInfo* virtualMethod;
	};


	struct IRRetVar_ret_1 : IRCommon
	{
		uint16_t ret;
	};


	struct IRRetVar_ret_2 : IRCommon
	{
		uint16_t ret;
	};


	struct IRRetVar_ret_4 : IRCommon
	{
		uint16_t ret;
	};


	struct IRRetVar_ret_8 : IRCommon
	{
		uint16_t ret;
	};


	struct IRRetVar_ret_12 : IRCommon
	{
		uint16_t ret;
	};


	struct IRRetVar_ret_16 : IRCommon
	{
		uint16_t ret;
	};


	struct IRRetVar_ret_20 : IRCommon
	{
		uint16_t ret;
	};


	struct IRRetVar_ret_24 : IRCommon
	{
		uint16_t ret;
	};


	struct IRRetVar_ret_28 : IRCommon
	{
		uint16_t ret;
	};


	struct IRRetVar_ret_32 : IRCommon
	{
		uint16_t ret;
	};


	struct IRRetVar_ret_n : IRCommon
	{
		uint16_t ret;
		uint32_t size;
	};


	struct IRRetVar_void : IRCommon
	{

	};


	struct IRCallNative_void : IRCommon
	{
		uint32_t managed2NativeMethod;
		uint32_t methodInfo;
		uint32_t argIdxs;
	};


	struct IRCallNative_ret : IRCommon
	{
		uint32_t managed2NativeMethod;
		uint32_t methodInfo;
		uint32_t argIdxs;
		uint16_t ret;
	};


	struct IRCallNative_ret_expand : IRCommon
	{
		uint32_t managed2NativeMethod;
		uint32_t methodInfo;
		uint32_t argIdxs;
		uint16_t ret;
		uint8_t retLocationType;
		uint8_t __pad__;
	};


	struct IRCallInterp_void : IRCommon
	{
		MethodInfo* methodInfo;
		uint16_t argBase;
	};


	struct IRCallInterp_ret : IRCommon
	{
		MethodInfo* methodInfo;
		uint16_t argBase;
		uint16_t ret;
	};


	struct IRCallVirtual_void : IRCommon
	{
		uint32_t managed2NativeMethod;
		uint32_t methodInfo;
		uint32_t argIdxs;
	};


	struct IRCallVirtual_ret : IRCommon
	{
		uint32_t managed2NativeMethod;
		uint32_t methodInfo;
		uint32_t argIdxs;
		uint16_t ret;
	};


	struct IRCallVirtual_ret_expand : IRCommon
	{
		uint32_t managed2NativeMethod;
		uint32_t methodInfo;
		uint32_t argIdxs;
		uint16_t ret;
		uint8_t retLocationType;
		uint8_t __pad__;
	};


	struct IRCallInterpVirtual_void : IRCommon
	{
		MethodInfo* method;
		uint16_t argBase;
	};


	struct IRCallInterpVirtual_ret : IRCommon
	{
		MethodInfo* method;
		uint16_t argBase;
		uint16_t ret;
	};


	struct IRCallInd_void : IRCommon
	{
		uint32_t managed2NativeMethod;
		uint32_t methodInfo;
		uint32_t argIdxs;
	};


	struct IRCallInd_ret : IRCommon
	{
		uint32_t managed2NativeMethod;
		uint32_t methodInfo;
		uint32_t argIdxs;
		uint16_t ret;
	};


	struct IRCallInd_ret_expand : IRCommon
	{
		uint32_t managed2NativeMethod;
		uint32_t methodInfo;
		uint32_t argIdxs;
		uint16_t ret;
		uint8_t retLocationType;
		uint8_t __pad__;
	};


	struct IRCallDelegate_void : IRCommon
	{
		uint32_t managed2NativeStaticMethod;
		uint32_t managed2NativeInstanceMethod;
		uint32_t argIdxs;
		uint16_t invokeParamCount;
	};


	struct IRCallDelegate_ret : IRCommon
	{
		uint32_t managed2NativeStaticMethod;
		uint32_t managed2NativeInstanceMethod;
		uint32_t argIdxs;
		uint16_t ret;
		uint16_t invokeParamCount;
	};


	struct IRCallDelegate_ret_expand : IRCommon
	{
		uint32_t managed2NativeStaticMethod;
		uint32_t managed2NativeInstanceMethod;
		uint32_t argIdxs;
		uint16_t ret;
		uint16_t invokeParamCount;
		uint8_t retLocationType;
		uint8_t __pad__;
	};


	struct IRNewDelegate : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t obj;
		uint16_t method;
	};


	struct IRBoxVarVar : IRCommon
	{
		uint16_t dst;
		uint16_t data;
		Il2CppClass* klass;
	};


	struct IRUnBoxVarVar : IRCommon
	{
		uint16_t addr;
		uint16_t obj;
		Il2CppClass* klass;
	};


	struct IRUnBoxAnyVarVar : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		Il2CppClass* klass;
	};


	struct IRCastclassVar : IRCommon
	{
		uint16_t obj;
		uint32_t klass;
	};


	struct IRIsInstVar : IRCommon
	{
		uint16_t obj;
		uint32_t klass;
	};


	struct IRLdtokenVar : IRCommon
	{
		uint16_t runtimeHandle;
		void* token;
	};


	struct IRMakeRefVarVar : IRCommon
	{
		uint16_t dst;
		uint16_t data;
		Il2CppClass* klass;
	};


	struct IRRefAnyTypeVarVar : IRCommon
	{
		uint16_t dst;
		uint16_t typedRef;
	};


	struct IRRefAnyValueVarVar : IRCommon
	{
		uint16_t addr;
		uint16_t typedRef;
		Il2CppClass* klass;
	};


	struct IRCpobjVarVar_1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRCpobjVarVar_2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRCpobjVarVar_4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRCpobjVarVar_8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRCpobjVarVar_12 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRCpobjVarVar_16 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRCpobjVarVar_20 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRCpobjVarVar_24 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRCpobjVarVar_28 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRCpobjVarVar_32 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRCpobjVarVar_n_2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
		uint16_t size;
	};


	struct IRCpobjVarVar_n_4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
		uint16_t size;
	};


	struct IRLdobjVarVar_1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdobjVarVar_2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdobjVarVar_4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdobjVarVar_8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdobjVarVar_12 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdobjVarVar_16 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdobjVarVar_20 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdobjVarVar_24 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdobjVarVar_28 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdobjVarVar_32 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRLdobjVarVar_n_4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
		uint16_t size;
	};


	struct IRStobjVarVar_1 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStobjVarVar_2 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStobjVarVar_4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStobjVarVar_8 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStobjVarVar_12 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStobjVarVar_16 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStobjVarVar_20 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStobjVarVar_24 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStobjVarVar_28 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStobjVarVar_32 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
	};


	struct IRStobjVarVar_n_4 : IRCommon
	{
		uint16_t dst;
		uint16_t src;
		uint16_t size;
	};


	struct IRInitobjVar_1 : IRCommon
	{
		uint16_t obj;
	};


	struct IRInitobjVar_2 : IRCommon
	{
		uint16_t obj;
	};


	struct IRInitobjVar_4 : IRCommon
	{
		uint16_t obj;
	};


	struct IRInitobjVar_8 : IRCommon
	{
		uint16_t obj;
	};


	struct IRInitobjVar_12 : IRCommon
	{
		uint16_t obj;
	};


	struct IRInitobjVar_16 : IRCommon
	{
		uint16_t obj;
	};


	struct IRInitobjVar_20 : IRCommon
	{
		uint16_t obj;
	};


	struct IRInitobjVar_24 : IRCommon
	{
		uint16_t obj;
	};


	struct IRInitobjVar_28 : IRCommon
	{
		uint16_t obj;
	};


	struct IRInitobjVar_32 : IRCommon
	{
		uint16_t obj;
	};


	struct IRInitobjVar_n_2 : IRCommon
	{
		uint16_t obj;
		uint16_t size;
	};


	struct IRInitobjVar_n_4 : IRCommon
	{
		uint16_t obj;
		uint32_t size;
	};


	struct IRLdstrVar : IRCommon
	{
		uint16_t dst;
		uint32_t str;
	};


	struct IRLdfldVarVar_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_size_8 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_size_12 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_size_16 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_size_20 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_size_24 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_size_28 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_size_32 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldVarVar_n_2 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
		uint16_t size;
	};


	struct IRLdfldVarVar_n_4 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
		uint32_t size;
	};


	struct IRLdfldValueTypeVarVar_i1 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_u1 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_i2 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_u2 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_i4 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_u4 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_i8 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_u8 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_size_8 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_size_12 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_size_16 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_size_20 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_size_24 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_size_28 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_size_32 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRLdfldValueTypeVarVar_n_2 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
		uint16_t size;
	};


	struct IRLdfldValueTypeVarVar_n_4 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
		uint32_t size;
	};


	struct IRLdfldaVarVar : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t offset;
	};


	struct IRStfldVarVar_i1 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_u1 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_i2 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_u2 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_i4 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_u4 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_i8 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_u8 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_size_8 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_size_12 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_size_16 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_size_20 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_size_24 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_size_28 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_size_32 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStfldVarVar_n_2 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
		uint16_t size;
	};


	struct IRStfldVarVar_n_4 : IRCommon
	{
		uint16_t obj;
		uint16_t offset;
		uint16_t data;
		uint32_t size;
	};


	struct IRLdsfldVarVar_i1 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_u1 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_i2 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_u2 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_i4 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_u4 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_i8 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_u8 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_size_8 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_size_12 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_size_16 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_size_20 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_size_24 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_size_28 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_size_32 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldVarVar_n_2 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t size;
	};


	struct IRLdsfldVarVar_n_4 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
		uint32_t size;
	};


	struct IRStsfldVarVar_i1 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_u1 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_i2 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_u2 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_i4 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_u4 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_i8 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_u8 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_size_8 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_size_12 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_size_16 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_size_20 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_size_24 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_size_28 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_size_32 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStsfldVarVar_n_2 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
		uint16_t size;
	};


	struct IRStsfldVarVar_n_4 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
		uint32_t size;
	};


	struct IRLdsfldaVarVar : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		uint16_t offset;
	};


	struct IRLdsfldaFromFieldDataVarVar : IRCommon
	{
		uint16_t dst;
		void* src;
	};


	struct IRLdthreadlocalaVarVar : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_i1 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_u1 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_i2 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_u2 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_i4 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_u4 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_i8 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_u8 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_size_8 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_size_12 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_size_16 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_size_20 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_size_24 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_size_28 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_size_32 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
	};


	struct IRLdthreadlocalVarVar_n_2 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
		uint16_t size;
	};


	struct IRLdthreadlocalVarVar_n_4 : IRCommon
	{
		uint16_t dst;
		Il2CppClass* klass;
		int32_t offset;
		uint32_t size;
	};


	struct IRStthreadlocalVarVar_i1 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_u1 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_i2 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_u2 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_i4 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_u4 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_i8 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_u8 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_size_8 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_size_12 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_size_16 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_size_20 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_size_24 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_size_28 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_size_32 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
	};


	struct IRStthreadlocalVarVar_n_2 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
		uint16_t size;
	};


	struct IRStthreadlocalVarVar_n_4 : IRCommon
	{
		Il2CppClass* klass;
		uint16_t offset;
		uint16_t data;
		uint32_t size;
	};


	struct IRNewArrVarVar_4 : IRCommon
	{
		uint16_t arr;
		uint16_t size;
		Il2CppClass* klass;
	};


	struct IRNewArrVarVar_8 : IRCommon
	{
		uint16_t arr;
		uint16_t size;
		Il2CppClass* klass;
	};


	struct IRGetArrayLengthVarVar_4 : IRCommon
	{
		uint16_t len;
		uint16_t arr;
	};


	struct IRGetArrayLengthVarVar_8 : IRCommon
	{
		uint16_t len;
		uint16_t arr;
	};


	struct IRGetArrayElementAddressAddrVarVar_i4 : IRCommon
	{
		uint16_t addr;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementAddressAddrVarVar_i8 : IRCommon
	{
		uint16_t addr;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementAddressCheckAddrVarVar_i4 : IRCommon
	{
		uint16_t addr;
		uint16_t arr;
		uint16_t index;
		Il2CppClass* eleKlass;
	};


	struct IRGetArrayElementAddressCheckAddrVarVar_i8 : IRCommon
	{
		uint16_t addr;
		uint16_t arr;
		uint16_t index;
		Il2CppClass* eleKlass;
	};


	struct IRGetArrayElementVarVar_i1_4 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_u1_4 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_i2_4 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_u2_4 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_i4_4 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_u4_4 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_i8_4 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_u8_4 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_size_12_4 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_size_16_4 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_n_4 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_i1_8 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_u1_8 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_i2_8 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_u2_8 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_i4_8 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_u4_8 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_i8_8 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_u8_8 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_size_12_8 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_size_16_8 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRGetArrayElementVarVar_n_8 : IRCommon
	{
		uint16_t dst;
		uint16_t arr;
		uint16_t index;
	};


	struct IRSetArrayElementVarVar_i1_4 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_u1_4 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_i2_4 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_u2_4 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_i4_4 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_u4_4 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_i8_4 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_u8_4 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_ref_4 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_size_12_4 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_size_16_4 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_n_4 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_i1_8 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_u1_8 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_i2_8 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_u2_8 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_i4_8 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_u4_8 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_i8_8 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_u8_8 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_ref_8 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_size_12_8 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_size_16_8 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRSetArrayElementVarVar_n_8 : IRCommon
	{
		uint16_t arr;
		uint16_t index;
		uint16_t ele;
	};


	struct IRNewMdArrVarVar_length : IRCommon
	{
		uint16_t arr;
		uint16_t lengthIdxs;
		Il2CppClass* klass;
	};


	struct IRNewMdArrVarVar_length_bound : IRCommon
	{
		uint16_t arr;
		uint16_t lengthIdxs;
		uint16_t lowerBoundIdxs;
		Il2CppClass* klass;
	};


	struct IRGetMdArrElementVarVar_i1 : IRCommon
	{
		uint16_t arr;
		uint16_t lengthIdxs;
		uint16_t value;
	};


	struct IRGetMdArrElementVarVar_u1 : IRCommon
	{
		uint16_t arr;
		uint16_t lengthIdxs;
		uint16_t value;
	};


	struct IRGetMdArrElementVarVar_i2 : IRCommon
	{
		uint16_t arr;
		uint16_t lengthIdxs;
		uint16_t value;
	};


	struct IRGetMdArrElementVarVar_u2 : IRCommon
	{
		uint16_t arr;
		uint16_t lengthIdxs;
		uint16_t value;
	};


	struct IRGetMdArrElementVarVar_i4 : IRCommon
	{
		uint16_t arr;
		uint16_t lengthIdxs;
		uint16_t value;
	};


	struct IRGetMdArrElementVarVar_u4 : IRCommon
	{
		uint16_t arr;
		uint16_t lengthIdxs;
		uint16_t value;
	};


	struct IRGetMdArrElementVarVar_i8 : IRCommon
	{
		uint16_t arr;
		uint16_t lengthIdxs;
		uint16_t value;
	};


	struct IRGetMdArrElementVarVar_u8 : IRCommon
	{
		uint16_t arr;
		uint16_t lengthIdxs;
		uint16_t value;
	};


	struct IRGetMdArrElementVarVar_size : IRCommon
	{
		uint16_t arr;
		uint16_t lengthIdxs;
		uint16_t value;
	};


	struct IRGetMdArrElementAddressVarVar : IRCommon
	{
		uint16_t addr;
		uint16_t arr;
		uint16_t lengthIdxs;
	};


	struct IRSetMdArrElementVarVar : IRCommon
	{
		uint16_t arr;
		uint16_t lengthIdxs;
		uint16_t value;
	};


	struct IRThrowEx : IRCommon
	{
		uint16_t exceptionObj;
	};


	struct IRRethrowEx : IRCommon
	{

	};


	struct IRLeaveEx : IRCommon
	{
		int32_t offset;
	};


	struct IREndFilterEx : IRCommon
	{
		uint16_t value;
	};


	struct IREndFinallyEx : IRCommon
	{

	};


	struct IRNullableNewVarVar : IRCommon
	{
		uint16_t dst;
		uint16_t data;
		Il2CppClass* klass;
	};


	struct IRNullableCtorVarVar : IRCommon
	{
		uint16_t dst;
		uint16_t data;
		Il2CppClass* klass;
	};


	struct IRNullableHasValueVar : IRCommon
	{
		uint16_t result;
		uint16_t obj;
		Il2CppClass* klass;
	};


	struct IRNullableGetValueOrDefaultVarVar : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		Il2CppClass* klass;
	};


	struct IRNullableGetValueOrDefaultVarVar_1 : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		uint16_t defaultValue;
		Il2CppClass* klass;
	};


	struct IRNullableGetValueVarVar : IRCommon
	{
		uint16_t dst;
		uint16_t obj;
		Il2CppClass* klass;
	};


	struct IRInterlockedCompareExchangeVarVarVarVar_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t location;
		uint16_t value;
		uint16_t comparand;
	};


	struct IRInterlockedCompareExchangeVarVarVarVar_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t location;
		uint16_t value;
		uint16_t comparand;
	};


	struct IRInterlockedCompareExchangeVarVarVarVar_pointer : IRCommon
	{
		uint16_t ret;
		uint16_t location;
		uint16_t value;
		uint16_t comparand;
	};


	struct IRInterlockedExchangeVarVarVar_i4 : IRCommon
	{
		uint16_t ret;
		uint16_t location;
		uint16_t value;
	};


	struct IRInterlockedExchangeVarVarVar_i8 : IRCommon
	{
		uint16_t ret;
		uint16_t location;
		uint16_t value;
	};


	struct IRInterlockedExchangeVarVarVar_pointer : IRCommon
	{
		uint16_t ret;
		uint16_t location;
		uint16_t value;
	};


	struct IRNewSystemObjectVar : IRCommon
	{
		uint16_t obj;
	};


	struct IRNewVector2 : IRCommon
	{
		uint16_t obj;
		uint16_t x;
		uint16_t y;
	};


	struct IRNewVector3_2 : IRCommon
	{
		uint16_t obj;
		uint16_t x;
		uint16_t y;
	};


	struct IRNewVector3_3 : IRCommon
	{
		uint16_t obj;
		uint16_t x;
		uint16_t y;
		uint16_t z;
	};


	struct IRNewVector4_2 : IRCommon
	{
		uint16_t obj;
		uint16_t x;
		uint16_t y;
	};


	struct IRNewVector4_3 : IRCommon
	{
		uint16_t obj;
		uint16_t x;
		uint16_t y;
		uint16_t z;
	};


	struct IRNewVector4_4 : IRCommon
	{
		uint16_t obj;
		uint16_t x;
		uint16_t y;
		uint16_t z;
		uint16_t w;
	};


	//!!!}}INST
#pragma pack(pop)

#pragma endregion

}
}