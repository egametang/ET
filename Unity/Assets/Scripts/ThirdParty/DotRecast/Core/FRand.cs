using System;

namespace DotRecast.Core
{
    public class FRand : IRcRand
    {
        private readonly Random _r;

        public FRand()
        {
            _r = new Random();
        }

        public FRand(long seed)
        {
            _r = new Random((int)seed); // TODO : 랜덤 시드 확인 필요
        }

        public float Next()
        {
            return (float)_r.NextDouble();
        }
    }
}