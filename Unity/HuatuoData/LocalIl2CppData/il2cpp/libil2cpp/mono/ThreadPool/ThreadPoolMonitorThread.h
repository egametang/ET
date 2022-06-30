#pragma once

enum MonitorStatus
{
    MONITOR_STATUS_REQUESTED,
    MONITOR_STATUS_WAITING_FOR_REQUEST,
    MONITOR_STATUS_NOT_RUNNING,
};

void monitor_ensure_running();
MonitorStatus GetMonitorStatus();
