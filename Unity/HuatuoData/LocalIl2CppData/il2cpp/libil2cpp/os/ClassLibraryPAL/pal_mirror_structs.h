#pragma once

/**
 * File status returned by Stat or FStat.
 */
struct FileStatus
{
    int32_t Flags;     // flags for testing if some members are present (see FileStatusFlags)
    int32_t Mode;      // file mode (see S_I* constants above for bit values)
    uint32_t Uid;      // user ID of owner
    uint32_t Gid;      // group ID of owner
    int64_t Size;      // total size, in bytes
    int64_t ATime;     // time of last access
    int64_t ATimeNsec; //     nanosecond part
    int64_t MTime;     // time of last modification
    int64_t MTimeNsec; //     nanosecond part
    int64_t CTime;     // time of last status change
    int64_t CTimeNsec; //     nanosecond part
    int64_t BirthTime; // time the file was created
    int64_t BirthTimeNsec; // nanosecond part
    int64_t Dev;       // ID of the device containing the file
    int64_t Ino;       // inode number of the file
    uint32_t UserFlags; // user defined flags
};
