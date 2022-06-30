#include "MonoPosixHelper.h"

#include <string.h>
#include <stdlib.h>

#include "../external/zlib/zlib.h"

#define BUFFER_SIZE 4096
#define ARGUMENT_ERROR -10
#define IO_ERROR -11

typedef int32_t (*read_write_func)(intptr_t buffer, int32_t length, intptr_t gchandle);

struct ZStream
{
    z_stream *stream;
    uint8_t *buffer;
    read_write_func func;
    void *gchandle;
    uint8_t compress;
    uint8_t eof;
};

static int32_t write_to_managed(ZStream *stream)
{
    int32_t n;
    z_stream *zs;

    zs = stream->stream;
    if (zs->avail_out != BUFFER_SIZE)
    {
        intptr_t buffer_ptr = reinterpret_cast<intptr_t>(stream->buffer);
        intptr_t gchandle_ptr = reinterpret_cast<intptr_t>(stream->gchandle);

        n = stream->func(buffer_ptr, BUFFER_SIZE - zs->avail_out, gchandle_ptr);
        zs->next_out = stream->buffer;
        zs->avail_out = BUFFER_SIZE;
        if (n < 0)
            return IO_ERROR;
    }
    return 0;
}

static int32_t flush_internal(ZStream *stream, bool is_final)
{
    int32_t status;

    if (!stream->compress)
        return 0;

    if (!is_final)
    {
        status = deflate(stream->stream, Z_PARTIAL_FLUSH);
        if (status != Z_OK && status != Z_STREAM_END)
            return status;
    }

    return write_to_managed(stream);
}

static void *z_alloc(void *opaque, uint32_t nitems, uint32_t item_size)
{
    return calloc(nitems, item_size);
}

static void z_free(void *opaque, void *ptr)
{
    free(ptr);
}

intptr_t CreateZStream(int32_t compress, uint8_t gzip, Il2CppMethodPointer func_ptr, intptr_t gchandle)
{
    z_stream *z;
    int32_t retval;
    ZStream *result;

    intptr_t result_ptr = 0;
    read_write_func func = (read_write_func)func_ptr;

    if (func == NULL)
        return result_ptr;

#if !defined(ZLIB_VERNUM) || (ZLIB_VERNUM < 0x1204)
    // Older versions of zlib do not support raw deflate or gzip
    return NULL;
#endif

    z = (z_stream*)calloc(1, sizeof(z_stream));
    if (compress)
    {
        retval = deflateInit2(z, Z_DEFAULT_COMPRESSION, Z_DEFLATED, gzip ? 31 : -15, 8, Z_DEFAULT_STRATEGY);
    }
    else
    {
        retval = inflateInit2(z, gzip ? 31 : -15);
    }

    if (retval != Z_OK)
    {
        free(z);
        return result_ptr;
    }

    z->zalloc = z_alloc;
    z->zfree = z_free;
    result = (ZStream*)calloc(1, sizeof(ZStream));
    result->stream = z;
    result->func = func;
    result->gchandle = reinterpret_cast<void*>(gchandle);
    result->compress = compress;
    result->buffer = (uint8_t*)malloc(BUFFER_SIZE * sizeof(uint8_t));

    result_ptr = reinterpret_cast<intptr_t>(result);
    return result_ptr;
}

int32_t CloseZStream(intptr_t zstream)
{
    int32_t status;
    int32_t flush_status;

    ZStream *stream = reinterpret_cast<ZStream*>(zstream);

    if (stream == NULL)
        return ARGUMENT_ERROR;

    status = 0;
    if (stream->compress)
    {
        if (stream->stream->total_in > 0)
        {
            do
            {
                status = deflate(stream->stream, Z_FINISH);
                flush_status = flush_internal(stream, true);
            }
            while (status == Z_OK); /* We want Z_STREAM_END or error here here */

            if (status == Z_STREAM_END)
                status = flush_status;
        }
        deflateEnd(stream->stream);
    }
    else
    {
        inflateEnd(stream->stream);
    }

    free(stream->buffer);
    free(stream->stream);
    memset(stream, 0, sizeof(ZStream));
    free(stream);

    return status;
}

int32_t Flush(intptr_t zstream)
{
    ZStream *stream = (ZStream*)zstream;
    return flush_internal(stream, false);
}

int32_t ReadZStream(intptr_t zstream, intptr_t zbuffer, int32_t length)
{
    int32_t n;
    int32_t status;
    z_stream *zs;

    ZStream *stream = (ZStream*)zstream;
    uint8_t *buffer = (uint8_t*)zbuffer;

    if (stream == NULL || buffer == NULL || length < 0)
        return ARGUMENT_ERROR;

    if (stream->eof)
        return 0;

    zs = stream->stream;
    zs->next_out = buffer;
    zs->avail_out = length;
    while (zs->avail_out > 0)
    {
        if (zs->avail_in == 0)
        {
            intptr_t buffer_ptr = reinterpret_cast<intptr_t>(stream->buffer);
            intptr_t gchandle_ptr = reinterpret_cast<intptr_t>(stream->gchandle);

            n = stream->func(buffer_ptr, BUFFER_SIZE, gchandle_ptr);
            if (n <= 0)
            {
                stream->eof = 1;
                break;
            }
            zs->next_in = stream->buffer;
            zs->avail_in = n;
        }

        status = inflate(stream->stream, Z_SYNC_FLUSH);
        if (status == Z_STREAM_END)
        {
            stream->eof = 1;
            break;
        }
        else if (status != Z_OK)
        {
            return status;
        }
    }
    return length - zs->avail_out;
}

int32_t WriteZStream(intptr_t zstream, intptr_t zbuffer, int32_t length)
{
    int32_t n;
    int32_t status;
    z_stream *zs;

    ZStream *stream = (ZStream*)zstream;
    uint8_t *buffer = (uint8_t*)zbuffer;

    if (stream == NULL || buffer == NULL || length < 0)
        return ARGUMENT_ERROR;

    if (stream->eof)
        return IO_ERROR;

    zs = stream->stream;
    zs->next_in = buffer;
    zs->avail_in = length;
    while (zs->avail_in > 0)
    {
        if (zs->avail_out == 0)
        {
            zs->next_out = stream->buffer;
            zs->avail_out = BUFFER_SIZE;
        }
        status = deflate(stream->stream, Z_NO_FLUSH);
        if (status != Z_OK && status != Z_STREAM_END)
            return status;

        if (zs->avail_out == 0)
        {
            n = write_to_managed(stream);
            if (n < 0)
                return n;
        }
    }
    return length;
}
