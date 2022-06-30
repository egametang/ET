#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX

#include "os/Console.h"
#include "os/File.h"

#include <stdlib.h>
#include <errno.h>
#include <fcntl.h>
#include <signal.h>
#include <stdio.h>
#include <termios.h>
#include <unistd.h>
#include <sys/ioctl.h>
#include <sys/time.h>
#include <sys/types.h>

namespace il2cpp
{
namespace os
{
namespace Console
{
#if !RUNTIME_TINY
    static bool setupComplete = false;
    static int32_t s_terminalSize;
    static struct termios s_initialAttr;
    static struct termios s_il2cppAttr;
    static std::string s_keypadXmit;
    static std::string s_teardown;
    static struct sigaction s_saveSigcont, s_saveSigint, s_saveSigwinch;

    static int32_t GetTerminalSize()
    {
        struct winsize ws;

        if (ioctl(STDIN_FILENO, TIOCGWINSZ, &ws) == 0)
            return (ws.ws_col << 16) | ws.ws_row;

        return -1;
    }

    static bool SetProperty(int32_t property, bool value)
    {
        struct termios attr;
        bool callset = false;
        bool check;

        if (tcgetattr(STDIN_FILENO, &attr) == -1)
            return false;

        check = (attr.c_lflag & property) != 0;
        if ((value || check) && !(value && check))
        {
            callset = true;
            if (value)
                attr.c_lflag |= property;
            else
                attr.c_lflag &= ~property;
        }

        if (!callset)
            return true;

        if (tcsetattr(STDIN_FILENO, TCSANOW, &attr) == -1)
            return true;

        s_il2cppAttr = attr;

        return true;
    }

    static void SetControlChars(uint8_t* control_chars, const uint8_t *cc)
    {
        // The index into the array comes from corlib/System/ControlCharacters.cs
#ifdef VINTR
        control_chars[0] = cc[VINTR];
#endif
#ifdef VQUIT
        control_chars[1] = cc[VQUIT];
#endif
#ifdef VERASE
        control_chars[2] = cc[VERASE];
#endif
#ifdef VKILL
        control_chars[3] = cc[VKILL];
#endif
#ifdef VEOF
        control_chars[4] = cc[VEOF];
#endif
#ifdef VTIME
        control_chars[5] = cc[VTIME];
#endif
#ifdef VMIN
        control_chars[6] = cc[VMIN];
#endif
#ifdef VSWTC
        control_chars[7] = cc[VSWTC];
#endif
#ifdef VSTART
        control_chars[8] = cc[VSTART];
#endif
#ifdef VSTOP
        control_chars[9] = cc[VSTOP];
#endif
#ifdef VSUSP
        control_chars[10] = cc[VSUSP];
#endif
#ifdef VEOL
        control_chars[11] = cc[VEOL];
#endif
#ifdef VREPRINT
        control_chars[12] = cc[VREPRINT];
#endif
#ifdef VDISCARD
        control_chars[13] = cc[VDISCARD];
#endif
#ifdef VWERASE
        control_chars[14] = cc[VWERASE];
#endif
#ifdef VLNEXT
        control_chars[15] = cc[VLNEXT];
#endif
#ifdef VEOL2
        control_chars[16] = cc[VEOL2];
#endif
    }

    static void CallDoConsoleCancelEvent()
    {
        // TODO: Call Console.cancel_handler delegate from another thread.
    }

    static void SigintHandler(int signo)
    {
        static bool insideSigint = false;

        if (insideSigint)
            return;

        insideSigint = true;
        CallDoConsoleCancelEvent();
        insideSigint = false;
    }

    static void SigcontHandler(int signo, siginfo_t *the_siginfo, void *data)
    {
        // Ignore error, there is not much we can do in the sigcont handler.
        tcsetattr(STDIN_FILENO, TCSANOW, &s_il2cppAttr);

        if (!s_keypadXmit.empty())
            write(STDOUT_FILENO, s_keypadXmit.c_str(), s_keypadXmit.length());

        // Call previous handler
        if (s_saveSigcont.sa_sigaction != NULL &&
            s_saveSigcont.sa_sigaction != (void*)SIG_DFL &&
            s_saveSigcont.sa_sigaction != (void*)SIG_IGN)
            (*s_saveSigcont.sa_sigaction)(signo, the_siginfo, data);
    }

    static void SigwinchHandler(int signo, siginfo_t *the_siginfo, void *data)
    {
        const int32_t size = GetTerminalSize();

        if (size != -1)
            s_terminalSize = size;

        // Call previous handler
        if (s_saveSigwinch.sa_sigaction != NULL &&
            s_saveSigwinch.sa_sigaction != (void*)SIG_DFL &&
            s_saveSigwinch.sa_sigaction != (void*)SIG_IGN)
            (*s_saveSigwinch.sa_sigaction)(signo, the_siginfo, data);
    }

    static void ConsoleSetupSignalHandler()
    {
        struct sigaction sigcont, sigint, sigwinch;

        memset(&sigcont, 0, sizeof(struct sigaction));
        memset(&sigint, 0, sizeof(struct sigaction));
        memset(&sigwinch, 0, sizeof(struct sigaction));

        // Continuing
        sigcont.sa_sigaction = SigcontHandler;
        sigcont.sa_flags = SA_SIGINFO;
        sigemptyset(&sigcont.sa_mask);
        sigaction(SIGCONT, &sigcont, &s_saveSigcont);

        // Interrupt handler
        sigint.sa_handler = SigintHandler;
        sigint.sa_flags = 0;
        sigemptyset(&sigint.sa_mask);
        sigaction(SIGINT, &sigint, &s_saveSigint);

        // Window size changed
        sigwinch.sa_sigaction = SigwinchHandler;
        sigwinch.sa_flags = SA_SIGINFO;
        sigemptyset(&sigwinch.sa_mask);
        sigaction(SIGWINCH, &sigwinch, &s_saveSigwinch);
    }

// Exists in Mono, but is unused.
    static void ConsoleRestoreSignalHandlers()
    {
        sigaction(SIGCONT, &s_saveSigcont, NULL);
        sigaction(SIGINT, &s_saveSigint, NULL);
        sigaction(SIGWINCH, &s_saveSigwinch, NULL);
    }

    int32_t InternalKeyAvailable(int32_t ms_timeout)
    {
        fd_set rfds;
        struct timeval tv;
        struct timeval *tvptr;
        div_t divvy;
        int32_t ret, nbytes;

        do
        {
            FD_ZERO(&rfds);
            FD_SET(STDIN_FILENO, &rfds);

            if (ms_timeout >= 0)
            {
                divvy = div(ms_timeout, 1000);
                tv.tv_sec = divvy.quot;
                tv.tv_usec = divvy.rem;
                tvptr = &tv;
            }
            else
            {
                tvptr = NULL;
            }

            ret = select(STDIN_FILENO + 1, &rfds, NULL, NULL, tvptr);
        }
        while (ret == -1 && errno == EINTR);

        if (ret > 0)
        {
            nbytes = 0;

            ret = ioctl(STDIN_FILENO, FIONREAD, &nbytes);

            if (ret >= 0)
                ret = nbytes;
        }

        return (ret > 0) ? ret : 0;
    }

    bool SetBreak(bool wantBreak)
    {
        return SetProperty(IGNBRK, !wantBreak);
    }

    bool SetEcho(bool wantEcho)
    {
        return SetProperty(ECHO, wantEcho);
    }

    static void TtyShutdown()
    {
        if (!setupComplete)
            return;

        if (!s_teardown.empty())
            write(STDOUT_FILENO, s_teardown.c_str(), s_teardown.length());

        tcflush(STDIN_FILENO, TCIFLUSH);
        tcsetattr(STDIN_FILENO, TCSANOW, &s_initialAttr);

        SetProperty(ECHO, true);

        setupComplete = false;
    }

    bool TtySetup(const std::string& keypadXmit, const std::string& teardown, uint8_t* control_characters, int32_t** size)
    {
        s_terminalSize = GetTerminalSize();

        if (s_terminalSize == -1)
        {
            int32_t cols = 0, rows = 0;

            const char *colsValue = getenv("COLUMNS");

            if (colsValue != NULL)
                cols = atoi(colsValue);

            const char *linesValue = getenv("LINES");

            if (linesValue != NULL)
                rows = atoi(linesValue);

            if (cols != 0 && rows != 0)
                s_terminalSize = (cols << 16) | rows;
            else
                s_terminalSize = -1;
        }

        *size = &s_terminalSize;

        if (tcgetattr(STDIN_FILENO, &s_initialAttr) == -1)
            return false;

        s_il2cppAttr = s_initialAttr;

        s_il2cppAttr.c_lflag &= ~(ICANON);
        s_il2cppAttr.c_iflag &= ~(IXON | IXOFF);
        s_il2cppAttr.c_cc[VMIN] = 1;
        s_il2cppAttr.c_cc[VTIME] = 0;

        if (tcsetattr(STDIN_FILENO, TCSANOW, &s_il2cppAttr) == -1)
            return false;

        s_keypadXmit = keypadXmit;

        SetControlChars(control_characters, s_il2cppAttr.c_cc);

        if (setupComplete)
            return true;

        ConsoleSetupSignalHandler();

        setupComplete = true;

        s_teardown = teardown;
        atexit(TtyShutdown);

        return true;
    }

#endif

    const char* NewLine()
    {
        return "\n";
    }
}
}
}

#endif
