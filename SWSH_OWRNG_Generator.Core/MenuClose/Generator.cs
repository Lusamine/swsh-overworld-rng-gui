using PKHeX.Core;
using SWSH_OWRNG_Generator.Core.Overworld;

namespace SWSH_OWRNG_Generator.Core.MenuClose
{
    public static class Generator
    {
        public static ref Xoroshiro128Plus Advance(ref Xoroshiro128Plus rng, uint NPCs, bool use_weather_fidgets, bool is_holding)
        {
            for (uint i = 0; i < NPCs; i++)
            {
                rng.NextInt(91);
            }
            if (!is_holding)
            {
                if (use_weather_fidgets)
                    rng.NextInt(); // rand 2 for weather
                rng.NextInt(61);
            }
            return ref rng;
        }
        public static uint GetAdvances(Xoroshiro128Plus rng, uint NPCs, bool use_weather_fidgets, bool is_holding)
        {
            (ulong _s0, ulong _s1) = rng.GetState();
            Advance(ref rng, NPCs, use_weather_fidgets, is_holding);

            uint c = 0;
            while (c < 500) // Prevent infinite loop, 500 is generous because even at 99 we shouldn't see higher than ~150
            {
                if (rng.GetState() == (_s0, _s1))
                {
                    break;
                }
                c++;
                rng.Prev();
            }

            return c;
        }
    }
}
