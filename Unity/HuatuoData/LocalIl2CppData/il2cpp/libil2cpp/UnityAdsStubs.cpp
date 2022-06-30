#if defined(__ANDROID__)

#include <cstddef>

#define DONTSTRIP __attribute__((used))
#define EXPORT __attribute__((visibility("default")))

extern "C"
{
    typedef void (*UnityAdsReadyCallback)(const char * placementId);
    typedef void (*UnityAdsDidErrorCallback)(long rawError, const char * message);
    typedef void (*UnityAdsDidStartCallback)(const char * placementId);
    typedef void (*UnityAdsDidFinishCallback)(const char * placementId, long rawFinishState);

    EXPORT DONTSTRIP void UnityAdsEngineInitialize(const char * gameId, bool testMode) {}
    EXPORT DONTSTRIP void UnityAdsEngineShow(const char * placementId) {}
    EXPORT DONTSTRIP bool UnityAdsEngineGetDebugMode() { return false; }
    EXPORT DONTSTRIP void UnityAdsEngineSetDebugMode(bool debugMode) {}
    EXPORT DONTSTRIP bool UnityAdsEngineIsSupported() { return false; }
    EXPORT DONTSTRIP bool UnityAdsEngineIsReady(const char * placementId) { return false; }
    EXPORT DONTSTRIP long UnityAdsEngineGetPlacementState(const char * placementId) { return -1; }
    EXPORT DONTSTRIP const char * UnityAdsEngineGetVersion() { return NULL; }
    EXPORT DONTSTRIP bool UnityAdsEngineIsInitialized() { return false; }
    EXPORT DONTSTRIP void UnityAdsEngineSetMetaData(const char * category, const char * data) {}
    EXPORT DONTSTRIP void UnityAdsEngineSetReadyCallback(UnityAdsReadyCallback callback) {}
    EXPORT DONTSTRIP void UnityAdsEngineSetDidErrorCallback(UnityAdsDidErrorCallback callback) {}
    EXPORT DONTSTRIP void UnityAdsEngineSetDidStartCallback(UnityAdsDidStartCallback callback) {}
    EXPORT DONTSTRIP void UnityAdsEngineSetDidFinishCallback(UnityAdsDidFinishCallback callback) {}
}

#endif
