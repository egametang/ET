#pragma once

struct Il2CppException;
namespace il2cpp
{
namespace utils
{
    NORETURN void RethrowException(Il2CppException* exception);

    template<typename FinallyBlock, bool isFault>
    struct FinallyHelper
    {
    public:
        inline FinallyHelper(FinallyBlock&& finallyBlock) :
            m_Exception(nullptr),
            // static cast to rvalue reference simulates std::move, as we don't want to include <utility> for all generated code
            m_FinallyBlock(static_cast<FinallyBlock &&>(finallyBlock))
        {
        }

        FinallyHelper(const FinallyHelper<FinallyBlock, isFault>&) = delete;
        FinallyHelper& operator=(const FinallyHelper<FinallyBlock, isFault>&) = delete;

        inline FinallyHelper(FinallyHelper<FinallyBlock, isFault>&& other) :
            m_FinallyBlock(static_cast<FinallyBlock &&>(other.m_FinallyBlock))
        {
        }

        inline FinallyHelper& operator=(FinallyHelper<FinallyBlock, isFault>&& other)
        {
            m_FinallyBlock = static_cast<FinallyBlock &&>(other.m_FinallyBlock);
            return *this;
        }

        inline ~FinallyHelper() noexcept(false)
        {
            if (isFault)
            {
                if (m_Exception != nullptr)
                {
                    m_FinallyBlock();
                    RethrowException(m_Exception);
                }
            }
            else
            {
                m_FinallyBlock();
                if (m_Exception != nullptr)
                    RethrowException(m_Exception);
            }
        }

        inline void StoreException(Il2CppException* exception)
        {
            m_Exception = exception;
        }

    private:
        Il2CppException* m_Exception;
        FinallyBlock m_FinallyBlock;
    };

    template<typename FinallyBlock>
    inline FinallyHelper<FinallyBlock, false> Finally(FinallyBlock&& finallyBlock)
    {
        return FinallyHelper<FinallyBlock, false>(static_cast<FinallyBlock &&>(finallyBlock));
    }

    template<typename FinallyBlock>
    inline FinallyHelper<FinallyBlock, true> Fault(FinallyBlock&& finallyBlock)
    {
        return FinallyHelper<FinallyBlock, true>(static_cast<FinallyBlock &&>(finallyBlock));
    }
}
}
