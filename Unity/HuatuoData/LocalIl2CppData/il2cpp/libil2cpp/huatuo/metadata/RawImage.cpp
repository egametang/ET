
#include "RawImage.h"

#include <cstring>
#include <cmath>
#include <iostream>

#include "il2cpp-class-internals.h"
#include "vm/GlobalMetadata.h"

#include "MetadataUtil.h"

namespace huatuo
{
	namespace metadata
	{

		bool RawImage::TranslateRVAToImageOffset(uint32_t rvaOffset, uint32_t& imageOffset)
		{
			const PESectionHeader* targetSection = nullptr;
			for (size_t i = 0; i < _PEHeader->sections; i++)
			{
				const PESectionHeader* ph = _PESectionHeaders + i;
				if ((byte*)ph >= _ptrRawDataEnd)
				{
					return false;
				}
				if (ph->virtualAddress <= rvaOffset && rvaOffset < ph->virtualAddress + ph->virtualSize)
				{
					imageOffset = ph->ptrRawData + rvaOffset - ph->virtualAddress;
					return true;
				}
			}
			return false;
		}

		const int kOptionalHeaderSize32 = 224;
		const int kOptionalHeaderSize64 = 240;
		const int kCliHeaderOffset32 = 208;
		const int kCliHeaderOffset64 = 224;

		LoadImageErrorCode RawImage::Load(const byte* imageData, size_t length)
		{
			_ptrRawData = imageData;
			_imageLength = uint32_t(length);
			_ptrRawDataEnd = imageData + length;

			const byte* imageDataEnd = imageData + length;
			const byte* ptr_lfanew = imageData + 0x3c;
			uint32_t lfanew = *(uint32_t*)ptr_lfanew;
			if (lfanew >= length)
			{
				return LoadImageErrorCode::BAD_IMAGE;
			}

			const byte* ptrSig = imageData + lfanew;
			/*if (ptr_sig[0] != 'P' || ptr_sig[1] != 'E' || ptr_sig[2] != 0 || ptr_sig[3] != 0)*/
			if (std::strncmp((const char*)ptrSig, "PE\0\0", 4))
			{
				return LoadImageErrorCode::BAD_IMAGE;
			}

			_PEHeader = (PEHeader*)(ptrSig + 4);

			_isDll = (_PEHeader->characteristics & 0x2000);
			// std::cout << "load " << (_isDll ? "dll" : "exe") << std::endl;

			// optional size may be 224(32bit matchine) or 240 (64bit)
			if (_PEHeader->optionalHeadersize != kOptionalHeaderSize32 && _PEHeader->optionalHeadersize != kOptionalHeaderSize64)
			{
				return LoadImageErrorCode::BAD_IMAGE;
			}
			bool is32BitFormat = _PEHeader->optionalHeadersize == kOptionalHeaderSize32;

			const PEDirEntry* ptrCLIHeaderEntry = (PEDirEntry*)(((byte*)_PEHeader)
				+ 20 /* pe header size */
				+ (is32BitFormat ? kCliHeaderOffset32 : kCliHeaderOffset64) /* pe optional header -> pe header data directories -> cli header */);


			_PESectionHeaders = (PESectionHeader*)((byte*)_PEHeader + 20 + _PEHeader->optionalHeadersize);

			uint32_t cLIHeaderOffset;
			if (!TranslateRVAToImageOffset(ptrCLIHeaderEntry->rva, cLIHeaderOffset))
			{
				return LoadImageErrorCode::BAD_IMAGE;
			}
			if (cLIHeaderOffset >= length)
			{
				return LoadImageErrorCode::BAD_IMAGE;
			}

			const CLIHeader* ptrCLIHeader = _CLIHeader = (const CLIHeader*)(imageData + cLIHeaderOffset);

			uint32_t metaOffset;
			if (!TranslateRVAToImageOffset(ptrCLIHeader->metaData.rva, metaOffset))
			{
				return LoadImageErrorCode::BAD_IMAGE;
			}
			if (metaOffset >= length)
			{
				return LoadImageErrorCode::BAD_IMAGE;
			}

			_ptrMetaRoot = (const MetadataRootPartial*)(imageData + metaOffset);
			if (_ptrMetaRoot->signature != 0x424A5342)
			{
				return LoadImageErrorCode::BAD_IMAGE;
			}
			//std::cout << "version:" << (const char*)&(_ptrMetaRoot->versionFirstByte) << std::endl;
			_ptrMetaData = (const byte*)_ptrMetaRoot;

			uint16_t numStreamHeader = *(uint16_t*)(_ptrMetaData + 16 + _ptrMetaRoot->length + 2);
			const StreamHeader* ptrStreamHeaders = (const StreamHeader*)(_ptrMetaData + 16 + _ptrMetaRoot->length + 4);

			const StreamHeader* curSH = ptrStreamHeaders;
			const size_t maxStreamNameSize = 16;
			for (int i = 0; i < numStreamHeader; i++)
			{
				//std::cout << "name:" << (char*)curSH->name << ", offset:" << curSH->offset << ", size:" << curSH->size << std::endl;

				if (curSH->offset >= ptrCLIHeader->metaData.size)
				{
					return LoadImageErrorCode::BAD_IMAGE;
				}
				CliStream* rs = nullptr;
				CliStream nonStandardStream;
				CliStream pdbStream;
				if (!std::strncmp(curSH->name, "#~", maxStreamNameSize))
				{
					rs = &_streamTables;
				}
				else if (!std::strncmp(curSH->name, "#Strings", maxStreamNameSize))
				{
					rs = &_streamStringHeap;
				}
				else if (!std::strncmp(curSH->name, "#US", maxStreamNameSize))
				{
					rs = &_streamUS;
				}
				else if (!std::strncmp(curSH->name, "#GUID", maxStreamNameSize))
				{
					rs = &_streamGuidHeap;
					if (curSH->size % 16 != 0)
					{
						return LoadImageErrorCode::BAD_IMAGE;
					}
				}
				else if (!std::strncmp(curSH->name, "#Blob", maxStreamNameSize))
				{
					rs = &_streamBlobHeap;
				}
				else if (!std::strncmp(curSH->name, "#-", maxStreamNameSize))
				{
					rs = &nonStandardStream;
				}
				else if (!std::strncmp(curSH->name, "#Pdb", maxStreamNameSize))
				{
					rs = &pdbStream;
				}
				else
				{
					std::cerr << "unknown stream name:" << curSH->name << std::endl;
					return LoadImageErrorCode::BAD_IMAGE;
				}
				rs->data = _ptrMetaData + curSH->offset;
				rs->size = curSH->size;
				rs->name = curSH->name;
				size_t sizeOfStream = 8 + (std::strlen(curSH->name) / 4 + 1) * 4;
				curSH = (const StreamHeader*)((byte*)curSH + sizeOfStream);
			}
			auto err = LoadStreams();
			if (err != LoadImageErrorCode::OK)
			{
				return err;
			}
			err = LoadTables();
			if (err != LoadImageErrorCode::OK)
			{
				return err;
			}
			return LoadImageErrorCode::OK;
		}

		LoadImageErrorCode RawImage::LoadTables()
		{
			const TableStreamHeader* ptrTableHeader = (const TableStreamHeader*)_streamTables.data;
			if (ptrTableHeader->reserved != 0 || ptrTableHeader->majorVersion != 2 || ptrTableHeader->minorVersion != 0)
			{
				return LoadImageErrorCode::BAD_IMAGE;
			}
			if ((ptrTableHeader->heapSizes & ~0x7))
			{
				return LoadImageErrorCode::BAD_IMAGE;
			}
			_4byteStringIndex = ptrTableHeader->heapSizes & 0x1;
			_4byteGUIDIndex = ptrTableHeader->heapSizes & 0x20;
			_4byteBlobIndex = ptrTableHeader->heapSizes & 0x4;

			uint64_t validMask = ((uint64_t)1 << TABLE_NUM) - 1;
			if (ptrTableHeader->valid & ~validMask)
			{
				return LoadImageErrorCode::BAD_IMAGE;
			}
			// sorted include not exist table, so check is not need.
			//if (ptrTableHeader->sorted & ~validMask)
			//{
			//	return LoadImageErrorCode::BAD_IMAGE;
			//}

			uint32_t validTableNum = GetNotZeroBitCount(ptrTableHeader->valid);
			//std::cout << "valid table num:" << validTableNum << std::endl;
			//printf("#~ size:%0x\n", _streamTables.size);
			const uint32_t* tableRowNums = (uint32_t*)(_streamTables.data + 24);
			const byte* tableDataBegin = _streamTables.data + 24 + 4 * validTableNum;

			{
				int curValidTableIndex = 0;
				for (int i = 0; i <= MAX_TABLE_INDEX; i++)
				{
					uint64_t mask = (uint64_t)1 << i;
					_tables[i] = {};
					if (ptrTableHeader->valid & mask)
					{
						uint32_t rowNum = tableRowNums[curValidTableIndex];
						_tables[i].rowNum = rowNum;
						++curValidTableIndex;
					}
				}
			}

			BuildTableRowMetas();

			int curValidTableIndex = 0;
			const byte* curTableData = tableDataBegin;
			for (int i = 0; i <= MAX_TABLE_INDEX; i++)
			{
				uint64_t mask = (uint64_t)1 << i;
				bool sorted = ptrTableHeader->sorted & mask;
				if (ptrTableHeader->valid & mask)
				{
					uint32_t rowNum = tableRowNums[curValidTableIndex];
					uint32_t metaDataRowSize = ComputTableRowMetaDataSize((TableType)i);
					uint64_t offset = curTableData - _ptrRawData;
					_tables[i] = { curTableData, metaDataRowSize, rowNum, true, sorted };
					curTableData += metaDataRowSize * rowNum;
					//std::cout << "table:" << i << " ," << curValidTableIndex << ", row_size:" << metaDataRowSize << ", row_num:" << rowNum << std::endl;
					//printf("table:[%d][%d] offset:%0llx row_size:%d row_count:%d\n", i, curValidTableIndex, offset, metaDataRowSize, rowNum);
					++curValidTableIndex;
				}
				else
				{
					_tables[i] = { nullptr, 0, 0, false, sorted };
				}
			}

			return LoadImageErrorCode::OK;
		}

		void RawImage::BuildTableRowMetas()
		{
			{
				auto& table = _tableRowMetas[(int)TableType::MODULE];
				table.push_back({ 2 });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputGUIDIndexByte() });
				table.push_back({ ComputGUIDIndexByte() });
				table.push_back({ ComputGUIDIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::TYPEREF];
				table.push_back({ ComputTableIndexByte(TableType::MODULE, TableType::MODULEREF, TableType::ASSEMBLYREF, TableType::TYPEREF, TagBits::ResoulutionScope) });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputStringIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::TYPEDEF];
				table.push_back({ 4 });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputTableIndexByte(TableType::TYPEDEF, TableType::TYPEREF, TableType::TYPESPEC, TagBits::TypeDefOrRef) });
				table.push_back({ ComputTableIndexByte(TableType::FIELD) });
				table.push_back({ ComputTableIndexByte(TableType::METHOD) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::FIELD_POINTER];
				table.push_back({ ComputTableIndexByte(TableType::FIELD) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::FIELD];
				table.push_back({ 2 });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputBlobIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::METHOD_POINTER];
				table.push_back({ ComputTableIndexByte(TableType::METHOD) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::METHOD];
				table.push_back({ 4 });
				table.push_back({ 2 });
				table.push_back({ 2 });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputBlobIndexByte() });
				table.push_back({ ComputTableIndexByte(TableType::PARAM) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::PARAM_POINTER];
				table.push_back({ ComputTableIndexByte(TableType::PARAM) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::PARAM];
				table.push_back({ 2 });
				table.push_back({ 2 });
				table.push_back({ ComputStringIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::INTERFACEIMPL];
				table.push_back({ ComputTableIndexByte(TableType::TYPEDEF) });
				table.push_back({ ComputTableIndexByte(TableType::TYPEDEF, TableType::TYPEREF, TableType::TYPESPEC, TagBits::TypeDefOrRef) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::MEMBERREF];
				table.push_back({ ComputTableIndexByte(TableType::METHOD, TableType::MODULEREF, TableType::TYPEDEF, TableType::TYPEREF, TagBits::MemberRefParent) });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputBlobIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::CONSTANT];
				table.push_back({ 2 });
				table.push_back({ ComputTableIndexByte(TableType::PARAM, TableType::FIELD, TableType::PROPERTY, TagBits::HasConstant) });
				table.push_back({ ComputBlobIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::CUSTOMATTRIBUTE];
				table.push_back({ ComputTableIndexByte(HasCustomAttributeAssociateTables, sizeof(HasCustomAttributeAssociateTables) / sizeof(TableType), TagBits::HasCustomAttribute) });
				table.push_back({ ComputTableIndexByte(TableType::METHOD, TableType::MEMBERREF, TagBits::CustomAttributeType) });
				table.push_back({ ComputBlobIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::FIELDMARSHAL];
				table.push_back({ ComputTableIndexByte(TableType::FIELD, TableType::PARAM, TagBits::HasFieldMarshal) });
				table.push_back({ ComputBlobIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::DECLSECURITY];
				table.push_back({ 2 });
				table.push_back({ ComputTableIndexByte(TableType::TYPEDEF, TableType::METHOD, TableType::ASSEMBLY, TagBits::HasDeclSecurity) });
				table.push_back({ ComputBlobIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::CLASSLAYOUT];
				table.push_back({ 2 });
				table.push_back({ 4 });
				table.push_back({ ComputTableIndexByte(TableType::TYPEDEF) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::FIELDLAYOUT];
				table.push_back({ 4 });
				table.push_back({ ComputTableIndexByte(TableType::FIELD) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::STANDALONESIG];
				table.push_back({ ComputBlobIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::EVENTMAP];
				table.push_back({ ComputTableIndexByte(TableType::TYPEDEF) });
				table.push_back({ ComputTableIndexByte(TableType::EVENT) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::EVENT_POINTER];
				table.push_back({ ComputTableIndexByte(TableType::EVENT) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::EVENT];
				table.push_back({ 2 });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputTableIndexByte(TableType::TYPEDEF, TableType::TYPEREF, TableType::TYPESPEC, TagBits::TypeDefOrRef) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::PROPERTYMAP];
				table.push_back({ ComputTableIndexByte(TableType::TYPEDEF) });
				table.push_back({ ComputTableIndexByte(TableType::PROPERTY) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::PROPERTY_POINTER];
				table.push_back({ ComputTableIndexByte(TableType::PROPERTY) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::PROPERTY];
				table.push_back({ 2 });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputBlobIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::METHODSEMANTICS];
				table.push_back({ 2 });
				table.push_back({ ComputTableIndexByte(TableType::METHOD) });
				table.push_back({ ComputTableIndexByte(TableType::EVENT, TableType::PROPERTY, TagBits::HasSemantics) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::METHODIMPL];
				table.push_back({ ComputTableIndexByte(TableType::TYPEDEF) });
				table.push_back({ ComputTableIndexByte(TableType::METHOD, TableType::MEMBERREF, TagBits::MethodDefOrRef) });
				table.push_back({ ComputTableIndexByte(TableType::METHOD, TableType::MEMBERREF, TagBits::MethodDefOrRef) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::MODULEREF];
				table.push_back({ ComputStringIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::TYPESPEC];
				table.push_back({ ComputBlobIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::IMPLMAP];
				table.push_back({ 2 });
				table.push_back({ ComputTableIndexByte(TableType::FIELD, TableType::METHOD, TagBits::MemberForwarded) });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputTableIndexByte(TableType::MODULEREF) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::FIELDRVA];
				table.push_back({ 4 });
				table.push_back({ ComputTableIndexByte(TableType::FIELD) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::ASSEMBLY];
				table.push_back({ 4 });
				table.push_back({ 2 });
				table.push_back({ 2 });
				table.push_back({ 2 });
				table.push_back({ 2 });
				table.push_back({ 4 });
				table.push_back({ ComputBlobIndexByte() });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputStringIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::ASSEMBLYPROCESSOR];
				table.push_back({ 4 });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::ASSEMBLYOS];
				table.push_back({ 4 });
				table.push_back({ 4 });
				table.push_back({ 4 });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::ASSEMBLYREF];
				table.push_back({ 2 });
				table.push_back({ 2 });
				table.push_back({ 2 });
				table.push_back({ 2 });
				table.push_back({ 4 });
				table.push_back({ ComputBlobIndexByte() });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputBlobIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::ASSEMBLYREFPROCESSOR];
				table.push_back({ 4 });
				table.push_back({ ComputTableIndexByte(TableType::ASSEMBLYREF) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::ASSEMBLYREFOS];
				table.push_back({ 4 });
				table.push_back({ 4 });
				table.push_back({ 4 });
				table.push_back({ ComputTableIndexByte(TableType::ASSEMBLYREF) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::FILE];
				table.push_back({ 4 });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputBlobIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::EXPORTEDTYPE];
				table.push_back({ 4 });
				table.push_back({ 4 });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputTableIndexByte(TableType::FILE, TableType::EXPORTEDTYPE, TableType::ASSEMBLY, TagBits::Implementation) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::MANIFESTRESOURCE];
				table.push_back({ 4 });
				table.push_back({ 4 });
				table.push_back({ ComputStringIndexByte() });
				table.push_back({ ComputTableIndexByte(TableType::FILE, TableType::ASSEMBLYREF, TagBits::Implementation) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::NESTEDCLASS];
				table.push_back({ ComputTableIndexByte(TableType::TYPEDEF) });
				table.push_back({ ComputTableIndexByte(TableType::TYPEDEF) });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::GENERICPARAM];
				table.push_back({ 2 });
				table.push_back({ 2 });
				table.push_back({ ComputTableIndexByte(TableType::TYPEDEF, TableType::METHOD, TagBits::TypeOrMethodDef) });
				table.push_back({ ComputStringIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::METHODSPEC];
				table.push_back({ ComputTableIndexByte(TableType::METHOD, TableType::MEMBERREF, TagBits::MethodDefOrRef) });
				table.push_back({ ComputBlobIndexByte() });
			}
			{
				auto& table = _tableRowMetas[(int)TableType::GENERICPARAMCONSTRAINT];
				table.push_back({ ComputTableIndexByte(TableType::GENERICPARAM) });
				table.push_back({ ComputTableIndexByte(TableType::TYPEDEF, TableType::TYPEREF, TableType::TYPESPEC, TagBits::TypeDefOrRef) });
			}


			for (int i = 0; i < TABLE_NUM; i++)
			{
				auto& table = _tableRowMetas[i];
				if (table.empty())
				{
					IL2CPP_ASSERT(_tables[i].rowNum == 0 && _tables[i].rowMetaDataSize == 0);
				}
				else
				{
					uint32_t totalSize = 0;
					for (auto& col : table)
					{
						col.offset = totalSize;
						totalSize += col.size;
					}
					uint32_t computSize = ComputTableRowMetaDataSize((TableType)i);
					if (computSize != totalSize)
					{
						IL2CPP_ASSERT(totalSize == computSize);
					}
				}
			}
		}

		LoadImageErrorCode RawImage::LoadStreams()
		{
			{
				uint32_t stringNum = 0;
				const byte* cur;
				for (cur = _streamStringHeap.data; cur < _streamStringHeap.data + _streamStringHeap.size; )
				{
					++stringNum;
					//std::cout << "#strings.[" << stringNum << "]  " << cur << std::endl;
					cur += std::strlen((const char*)cur) + 1;
				}
				if (cur != _streamStringHeap.data + _streamStringHeap.size)
				{
					//std::cerr << "bad #strings" << std::endl;
					return LoadImageErrorCode::BAD_IMAGE;
				}
				//std::cout << "=== #String. num:" << stringNum << std::endl;

				_stringHeapStrNum = stringNum;
			}


			{
				//std::cout << "=== #GUID. num:" << _streamGuidHeap.size / 16 << std::endl;
			}
			{
				const byte* cur;
				uint32_t usNum = 0;
				uint32_t lengthSize;
				for (cur = _streamUS.data; cur < _streamUS.data + _streamUS.size;)
				{
					++usNum;
					uint32_t stringLength = BlobReader::ReadCompressedUint32(cur, lengthSize);
					cur += lengthSize;
					//std::cout << "#us.[" << usNum << "].size:" << stringLength << std::endl;
					cur += stringLength;
				}
				if (cur != _streamUS.data + _streamUS.size)
				{
					//std::cerr << "bad #US" << std::endl;
					return LoadImageErrorCode::BAD_IMAGE;
				}
				//std::cout << "=== #US. num:" << usNum << std::endl;

				_userStringStrNum = usNum;
			}

			{
				const byte* cur;
				uint32_t blobNum = 0;
				uint32_t lengthSize;
				for (cur = _streamBlobHeap.data; cur < _streamBlobHeap.data + _streamBlobHeap.size;)
				{
					++blobNum;
					uint32_t stringLength = BlobReader::ReadCompressedUint32(cur, lengthSize);
					cur += lengthSize;
					//std::cout << "#blob.[" << blobNum << "].size:" << stringLength << std::endl;
					cur += stringLength;
				}
				if (cur != _streamBlobHeap.data + _streamBlobHeap.size)
				{
					//std::cerr << "bad #Blob" << std::endl;
					return LoadImageErrorCode::BAD_IMAGE;
				}
				//std::cout << "=== #Blob. num:" << blobNum << std::endl;
				_blobNum = blobNum;
				return LoadImageErrorCode::OK;
			}

		}


		uint32_t RawImage::ComputTableRowMetaDataSize(TableType tableIndex) const
		{
			switch (tableIndex)
			{
			case TableType::MODULE: return 2 + ComputStringIndexByte() + ComputGUIDIndexByte() * 3;
			case TableType::TYPEREF: return ComputTableIndexByte(TableType::MODULE, TableType::MODULEREF, TableType::ASSEMBLYREF, TableType::TYPEREF, TagBits::ResoulutionScope)
				+ ComputStringIndexByte() * 2;
			case TableType::TYPEDEF: return 4 + ComputStringIndexByte() * 2
				+ ComputTableIndexByte(TableType::TYPEDEF, TableType::TYPEREF, TableType::TYPESPEC, TagBits::TypeDefOrRef)
				+ ComputTableIndexByte(TableType::FIELD)
				+ ComputTableIndexByte(TableType::METHOD);
			case TableType::FIELD_POINTER: return ComputTableIndexByte(TableType::FIELD);
			case TableType::FIELD: return 2 + ComputStringIndexByte() + ComputBlobIndexByte();
			case TableType::METHOD_POINTER: return ComputTableIndexByte(TableType::METHOD);
			case TableType::METHOD: return 4 + 2 + 2 + ComputStringIndexByte() + ComputBlobIndexByte() + ComputTableIndexByte(TableType::PARAM);
			case TableType::PARAM_POINTER: return ComputTableIndexByte(TableType::PARAM);
			case TableType::PARAM: return 2 + 2 + ComputStringIndexByte();
			case TableType::INTERFACEIMPL: return ComputTableIndexByte(TableType::TYPEDEF)
				+ ComputTableIndexByte(TableType::TYPEDEF, TableType::TYPEREF, TableType::TYPESPEC, TagBits::TypeDefOrRef);
			case TableType::MEMBERREF: return ComputTableIndexByte(TableType::METHOD, TableType::MODULEREF, TableType::TYPEDEF, TableType::TYPEREF, TagBits::MemberRefParent)
				+ ComputStringIndexByte() + ComputBlobIndexByte();
			case TableType::CONSTANT: return 2
				+ ComputTableIndexByte(TableType::PARAM, TableType::FIELD, TableType::PROPERTY, TagBits::HasConstant)
				+ ComputBlobIndexByte();
			case TableType::CUSTOMATTRIBUTE: return ComputTableIndexByte(HasCustomAttributeAssociateTables, sizeof(HasCustomAttributeAssociateTables) / sizeof(TableType), TagBits::HasCustomAttribute)
				+ ComputTableIndexByte(TableType::METHOD, TableType::MEMBERREF, TagBits::CustomAttributeType)
				+ ComputBlobIndexByte();
			case TableType::FIELDMARSHAL: return ComputTableIndexByte(TableType::FIELD, TableType::PARAM, TagBits::HasFieldMarshal)
				+ ComputBlobIndexByte();
			case TableType::DECLSECURITY: return 2 + ComputTableIndexByte(TableType::TYPEDEF, TableType::METHOD, TableType::ASSEMBLY, TagBits::HasDeclSecurity)
				+ ComputBlobIndexByte();
			case TableType::CLASSLAYOUT: return 2 + 4 + ComputTableIndexByte(TableType::TYPEDEF);
			case TableType::FIELDLAYOUT: return 4 + ComputTableIndexByte(TableType::FIELD);
			case TableType::STANDALONESIG: return ComputBlobIndexByte();
			case TableType::EVENTMAP: return ComputTableIndexByte(TableType::TYPEDEF) + ComputTableIndexByte(TableType::EVENT);
			case TableType::EVENT_POINTER: return ComputTableIndexByte(TableType::EVENT);
			case TableType::EVENT: return 2
				+ ComputStringIndexByte()
				+ ComputTableIndexByte(TableType::TYPEDEF, TableType::TYPEREF, TableType::TYPESPEC, TagBits::TypeDefOrRef);
			case TableType::PROPERTYMAP: return ComputTableIndexByte(TableType::TYPEDEF)
				+ ComputTableIndexByte(TableType::PROPERTY);
			case TableType::PROPERTY_POINTER: return ComputTableIndexByte(TableType::PROPERTY);
			case TableType::PROPERTY: return 2 + ComputStringIndexByte() + ComputBlobIndexByte();
			case TableType::METHODSEMANTICS: return 2
				+ ComputTableIndexByte(TableType::METHOD)
				+ ComputTableIndexByte(TableType::EVENT, TableType::PROPERTY, TagBits::HasSemantics);
			case TableType::METHODIMPL: return ComputTableIndexByte(TableType::TYPEDEF)
				+ ComputTableIndexByte(TableType::METHOD, TableType::MEMBERREF, TagBits::MethodDefOrRef)
				+ ComputTableIndexByte(TableType::METHOD, TableType::MEMBERREF, TagBits::MethodDefOrRef);
			case TableType::MODULEREF: return ComputStringIndexByte();
			case TableType::TYPESPEC: return ComputBlobIndexByte();
			case TableType::IMPLMAP: return 2
				+ ComputTableIndexByte(TableType::FIELD, TableType::METHOD, TagBits::MemberForwarded)
				+ ComputStringIndexByte()
				+ ComputTableIndexByte(TableType::MODULEREF);
			case TableType::FIELDRVA: return 4 + ComputTableIndexByte(TableType::FIELD);
			case TableType::UNUSED6:
			case TableType::UNUSED7:
				LogPanic("unused table type");
				return 0;
			case TableType::ASSEMBLY: return 4 + 4 * 2 + 4
				+ ComputBlobIndexByte()
				+ ComputStringIndexByte() * 2;
			case TableType::ASSEMBLYPROCESSOR: return 4;
			case TableType::ASSEMBLYOS: return 4 + 4 + 4;
			case TableType::ASSEMBLYREF: return 2 * 4 + 4
				+ ComputBlobIndexByte()
				+ ComputStringIndexByte() * 2
				+ ComputBlobIndexByte();
			case TableType::ASSEMBLYREFPROCESSOR: return 4 + ComputTableIndexByte(TableType::ASSEMBLYREF);
			case TableType::ASSEMBLYREFOS: return 4 * 3 + ComputTableIndexByte(TableType::ASSEMBLYREF);
			case TableType::FILE: return 4 + ComputStringIndexByte() + ComputBlobIndexByte();
			case TableType::EXPORTEDTYPE: return 4 + 4
				+ ComputStringIndexByte() * 2
				+ ComputTableIndexByte(TableType::FILE, TableType::EXPORTEDTYPE, TableType::ASSEMBLY, TagBits::Implementation);
			case TableType::MANIFESTRESOURCE: return 4 + 4
				+ ComputStringIndexByte()
				+ ComputTableIndexByte(TableType::FILE, TableType::ASSEMBLYREF, TagBits::Implementation);
			case TableType::NESTEDCLASS: return ComputTableIndexByte(TableType::TYPEDEF) * 2;
			case TableType::GENERICPARAM: return 2 + 2
				+ ComputTableIndexByte(TableType::TYPEDEF, TableType::METHOD, TagBits::TypeOrMethodDef)
				+ ComputStringIndexByte();
			case TableType::METHODSPEC: return ComputTableIndexByte(TableType::METHOD, TableType::MEMBERREF, TagBits::MethodDefOrRef)
				+ ComputBlobIndexByte();
			case TableType::GENERICPARAMCONSTRAINT: return ComputTableIndexByte(TableType::GENERICPARAM)
				+ ComputTableIndexByte(TableType::TYPEDEF, TableType::TYPEREF, TableType::TYPESPEC, TagBits::TypeDefOrRef);
			case TableType::UNUSED8:
			case TableType::UNUSED9:
			case TableType::UNUSED10:
				LogPanic("unused table type");
				return 0;
			case TableType::DOCUMENT: // return 84;
			case TableType::METHODBODY: // return 52;
			case TableType::LOCALSCOPE:  // return 20;
			case TableType::LOCALVARIABLE: // return 56;
			case TableType::LOCALCONSTANT: // return 24;
			case TableType::IMPORTSCOPE: // return 8;
			case TableType::STATEMACHINEMETHOD:
			case TableType::CUSTOMDEBUGINFORMATION:
				LogPanic("not support pdb table");
				return 0;
			default:
				LogPanic("unknown table type");
				return 0;
			}
		}

	}
}