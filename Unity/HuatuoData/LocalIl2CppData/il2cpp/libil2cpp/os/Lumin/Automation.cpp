#include "il2cpp-config.h"
#include "il2cpp-api.h"

#if IL2CPP_TARGET_LUMIN && IL2CPP_TARGET_LUMIN_AUTOMATION
#include "Automation.h"

#include <atomic>
#include <chrono>
#include <thread>

#ifndef EGL_EGLEXT_PROTOTYPES
#define EGL_EGLEXT_PROTOTYPES
#endif

#include <EGL/egl.h>
#include <EGL/eglext.h>

#ifndef GL_GLEXT_PROTOTYPES
#define GL_GLEXT_PROTOTYPES
#endif

#include <GLES3/gl3.h>
#include <GLES3/gl3ext.h>

#define ML_DEFAULT_LOG_TAG "il2cpp"

#include "ml_graphics.h"
#include "ml_head_tracking.h"
#include "ml_lifecycle.h"
#include "ml_logging.h"
#include "ml_perception.h"

static void onStopWrapper(void* app_ctx);
static void onPauseWrapper(void *app_ctx);
static void onResumeWrapper(void *app_ctx);

static std::thread s_app_thread;

namespace il2cpp
{
namespace os
{
namespace lumin
{
namespace automation
{
    class app_ctx_t;

    static app_ctx_t* s_app;

    class app_ctx_t
    {
    public:
        app_ctx_t();
        ~app_ctx_t();

        bool init();

        void main_loop();
        void on_stop();
        void on_pause();
        void on_resume();

        void makeCurrent();
        void releaseCurrent();
    private:
        EGLContext context;
        EGLDisplay display;

        MLHandle graphics_client;

        std::atomic_int op_mode;
    };

    app_ctx_t::app_ctx_t() :
        graphics_client(ML_INVALID_HANDLE),
        op_mode(0)
    {
        display = eglGetDisplay(EGL_DEFAULT_DISPLAY);

        eglInitialize(display, nullptr, nullptr);
        eglBindAPI(EGL_OPENGL_ES_API);

        EGLint config_attribs[] = {
            EGL_RED_SIZE, 5,
            EGL_GREEN_SIZE, 6,
            EGL_BLUE_SIZE, 5,
            EGL_ALPHA_SIZE, 0,
            EGL_DEPTH_SIZE, 24,
            EGL_STENCIL_SIZE, 8,
            EGL_NONE
        };
        EGLConfig egl_config = nullptr;
        EGLint config_size = 0;
        eglChooseConfig(display, config_attribs, &egl_config, 1, &config_size);

        EGLint context_attribs[] = {
            EGL_CONTEXT_MAJOR_VERSION_KHR, 3,
            EGL_CONTEXT_MINOR_VERSION_KHR, 0,
            EGL_NONE
        };
        context = eglCreateContext(display, egl_config, EGL_NO_CONTEXT, context_attribs);
    }

    app_ctx_t::~app_ctx_t()
    {
        eglDestroyContext(display, context);
        eglTerminate(display);
    }

    bool app_ctx_t::init()
    {
        MLLifecycleCallbacks cbs = {0};
        cbs.on_stop = &onStopWrapper;
        cbs.on_pause = &onPauseWrapper;
        cbs.on_resume = &onResumeWrapper;

        if (MLLifecycleInit(&cbs, static_cast<void*>(this)) != MLResult_Ok)
        {
            ML_LOG(Error, "[il2cpp_automation]: failed to initialize lifecycle");
            return false;
        }

        // initialize perception system
        MLPerceptionSettings perception_settings;
        if (MLResult_Ok != MLPerceptionInitSettings(&perception_settings))
        {
            ML_LOG(Error, "[il2cpp_automation]: Failed to initialize perception.");
        }

        if (MLResult_Ok != MLPerceptionStartup(&perception_settings))
        {
            ML_LOG(Error, "[il2cpp_automation]: Failed to startup perception.");
            return false;
        }

        return true;
    }

    void app_ctx_t::main_loop()
    {
        on_resume();

        makeCurrent();

        // Get ready to connect our GL context to the MLSDK graphics API
        MLGraphicsOptions graphics_options = { 0, MLSurfaceFormat_RGBA8UNorm, MLSurfaceFormat_D32Float };
        MLHandle opengl_context = reinterpret_cast<MLHandle>(context);

        MLGraphicsCreateClientGL(&graphics_options, opengl_context, &graphics_client);

        GLuint framebuffer_id;
        glGenFramebuffers(1, &framebuffer_id);

        MLHandle head_tracker;
        MLResult head_track_result = MLHeadTrackingCreate(&head_tracker);
        MLHeadTrackingStaticData head_static_data;
        if (MLResult_Ok == head_track_result && MLHandleIsValid(head_tracker))
        {
            MLHeadTrackingGetStaticData(head_tracker, &head_static_data);
        }
        else
        {
            ML_LOG(Error, "[il2cpp_automation]: Failed to create head tracker.");
        }

        ML_LOG(Info, "[il2cpp_automation]: Start loop.");

        auto start = std::chrono::steady_clock::now();

        do
        {
            MLGraphicsFrameParams frame_params;

            MLResult out_result = MLGraphicsInitFrameParams(&frame_params);
            if (MLResult_Ok != out_result)
            {
                ML_LOG(Error, "MLGraphicsInitFrameParams complained: %d", out_result);
            }
            frame_params.surface_scale = 1.0f;
            frame_params.far_clip = 100.0f;
            frame_params.near_clip = 0.37f;
            frame_params.focus_distance = 100.0f;

            MLHandle frame_handle = ML_INVALID_HANDLE;
            MLGraphicsVirtualCameraInfoArray virtual_camera_array;
            out_result = MLGraphicsBeginFrame(graphics_client, &frame_params, &frame_handle, &virtual_camera_array);
            if (MLResult_Ok != out_result)
            {
                static bool loggedBeginFrameFailure = false;
                if (!loggedBeginFrameFailure)
                {
                    ML_LOG(Error, "MLGraphicsInitFrameParams complained: %d", out_result);
                    loggedBeginFrameFailure = true;
                }
                if (((int)op_mode) > 0)
                    continue; // early out if we fail to acquire a frame.
                else
                    break;
            }

            auto msRuntime = std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::steady_clock::now() - start).count();
            auto factor = labs(msRuntime % 2000 - 1000) / 1000.0;

            for (int camera = 0; camera < 2; ++camera)
            {
                glBindFramebuffer(GL_FRAMEBUFFER, framebuffer_id);
                glFramebufferTextureLayer(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, virtual_camera_array.color_id, 0, camera);
                glFramebufferTextureLayer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, virtual_camera_array.depth_id, 0, camera);

                const MLRectf& viewport = virtual_camera_array.viewport;
                glViewport((GLint)viewport.x, (GLint)viewport.y,
                    (GLsizei)viewport.w, (GLsizei)viewport.h);
                glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
                if (camera == 0)
                {
                    glClearColor(1.0 - factor, 0.0, 0.0, 0.0);
                }
                else
                {
                    glClearColor(0.0, 0.0, factor, 0.0);
                }
                glBindFramebuffer(GL_FRAMEBUFFER, 0);
                MLGraphicsSignalSyncObjectGL(graphics_client, virtual_camera_array.virtual_cameras[camera].sync_object);
            }
            out_result = MLGraphicsEndFrame(graphics_client, frame_handle);
            if (MLResult_Ok != out_result)
            {
                ML_LOG(Error, "MLGraphicsEndFrame complained: %d", out_result);
            }
        }
        while (((int)op_mode) > 0);

        ML_LOG(Info, "[il2cpp_automation]: End loop.");

        glDeleteFramebuffers(1, &framebuffer_id);

        releaseCurrent();

        // clean up system
        MLGraphicsDestroyClient(&graphics_client);
        MLPerceptionShutdown();
    }

    void app_ctx_t::on_stop()
    {
        op_mode = 0;
    }

    void app_ctx_t::on_pause()
    {
        op_mode = 1;
    }

    void app_ctx_t::on_resume()
    {
        op_mode = 2;
    }

    void app_ctx_t::makeCurrent()
    {
        eglMakeCurrent(display, EGL_NO_SURFACE, EGL_NO_SURFACE, context);
    }

    void app_ctx_t::releaseCurrent()
    {
        eglMakeCurrent(NULL, EGL_NO_SURFACE, EGL_NO_SURFACE, NULL);
    }

    void Bootstrap()
    {
        il2cpp_set_data_dir("/package/Data");
        il2cpp_set_config_dir("/package/Data/etc");
        s_app = new app_ctx_t;
        if (s_app->init())
        {
            s_app_thread = std::thread(&app_ctx_t::main_loop, s_app);
        }
        MLLifecycleSetReadyIndication();
    }

    void WaitForAppThread()
    {
        if (s_app)
            s_app->on_stop();
        if (s_app_thread.joinable())
            s_app_thread.join();
        delete s_app;
        s_app = nullptr;
    }
}
}
}
}

static void onStopWrapper(void* app_ctx)
{
    auto ctx = static_cast<il2cpp::os::lumin::automation::app_ctx_t*>(app_ctx);
    ctx->on_stop();
}

static void onPauseWrapper(void *app_ctx)
{
    auto ctx = static_cast<il2cpp::os::lumin::automation::app_ctx_t*>(app_ctx);
    ctx->on_pause();
}

static void onResumeWrapper(void *app_ctx)
{
    auto ctx = static_cast<il2cpp::os::lumin::automation::app_ctx_t*>(app_ctx);
    ctx->on_resume();
}

#endif //IL2CPP_TARGET_LUMIN_AUTOMATION
