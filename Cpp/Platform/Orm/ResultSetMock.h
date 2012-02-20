// Copyright: All Rights Reserved
// Author: egametang@gmail.com (tanghai)

#ifndef ORM_RESULTSETMOCK_H
#define ORM_RESULTSETMOCK_H

#include <gmock/gmock.h>
#include <cppconn/resultset.h>

namespace Egametang {

class ResultSetMock: public sql::ResultSet
{
public:
	ResultSetMock()
	{
	}
	MOCK_METHOD1(absolute, bool(int));
	MOCK_METHOD0(afterLast, void());
	MOCK_METHOD0(beforeFirst, void());
	MOCK_METHOD0(cancelRowUpdates, void());
	MOCK_METHOD0(clearWarnings, void());
	MOCK_METHOD0(close, void());
	MOCK_CONST_METHOD1(findColumn, uint32_t(const sql::SQLString&));
	MOCK_METHOD0(first, bool());
	MOCK_CONST_METHOD1(getBlob, std::istream*(uint32_t));
	MOCK_CONST_METHOD1(getBlob, std::istream*(const sql::SQLString&));
	MOCK_CONST_METHOD1(getBoolean, bool(uint32_t));
	MOCK_CONST_METHOD1(getBoolean, bool(const sql::SQLString&));
	MOCK_METHOD0(getConcurrency, int());
	MOCK_METHOD0(getCursorName, sql::SQLString());
	MOCK_CONST_METHOD1(getDouble, long double(uint32_t));
	MOCK_CONST_METHOD1(getDouble, long double(const sql::SQLString&));
	MOCK_METHOD0(getFetchDirection, int());
	MOCK_METHOD0(getFetchSize, size_t());
	MOCK_METHOD0(getHoldability, int());
	MOCK_CONST_METHOD1(getInt, int32_t(uint32_t));
	MOCK_CONST_METHOD1(getInt, int32_t(const sql::SQLString&));
	MOCK_CONST_METHOD1(getUInt, uint32_t(uint32_t));
	MOCK_CONST_METHOD1(getUInt, uint32_t(const sql::SQLString&));
	MOCK_CONST_METHOD1(getInt64, int64_t(uint32_t));
	MOCK_CONST_METHOD1(getInt64, int64_t(const sql::SQLString&));
	MOCK_CONST_METHOD1(getUInt64, uint64_t(uint32_t));
	MOCK_CONST_METHOD1(getUInt64, uint64_t(const sql::SQLString&));
	MOCK_CONST_METHOD0(getMetaData, sql::ResultSetMetaData*());
	MOCK_CONST_METHOD0(getRow, size_t());
	MOCK_METHOD1(getRowId, sql::RowID*(uint32_t));
	MOCK_METHOD1(getRowId, sql::RowID*(const sql::SQLString&));
	MOCK_CONST_METHOD0(getStatement, const sql::Statement*());
	MOCK_CONST_METHOD1(getString, sql::SQLString(uint32_t));
	MOCK_CONST_METHOD1(getString, sql::SQLString(const sql::SQLString&));
	MOCK_CONST_METHOD0(getType, enum_type());
	MOCK_METHOD0(getWarnings, void());
	MOCK_METHOD0(insertRow, void());
	MOCK_CONST_METHOD0(isAfterLast, bool());
	MOCK_CONST_METHOD0(isBeforeFirst, bool());
	MOCK_CONST_METHOD0(isClosed, bool());
	MOCK_CONST_METHOD0(isFirst, bool());
	MOCK_CONST_METHOD0(isLast, bool());
	MOCK_CONST_METHOD1(isNull, bool(uint32_t));
	MOCK_CONST_METHOD1(isNull, bool(const sql::SQLString&));
	MOCK_METHOD0(last, bool());
	MOCK_METHOD0(next, bool());
	MOCK_METHOD0(moveToCurrentRow, void());
	MOCK_METHOD0(moveToInsertRow, void());
	MOCK_METHOD0(previous, bool());
	MOCK_METHOD0(refreshRow, void());
	MOCK_METHOD1(relative, bool(int));
	MOCK_METHOD0(rowDeleted, bool());
	MOCK_METHOD0(rowInserted, bool());
	MOCK_METHOD0(rowUpdated, bool());
	MOCK_METHOD1(setFetchSize, void(size_t));
	MOCK_CONST_METHOD0(rowsCount, size_t());
	MOCK_CONST_METHOD0(wasNull, bool());
};

} // namespace Egametang
#endif // ORM_RESULTSETMOCK_H
