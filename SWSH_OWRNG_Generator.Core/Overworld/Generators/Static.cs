﻿using PKHeX.Core;

namespace SWSH_OWRNG_Generator.Core.Overworld.Generators
{
    public static class Static
    {
        public static List<Frame> Generate(ulong state0, ulong state1, ulong advances, ulong InitialAdvances, IProgress<int> progress, Filter Filters, uint NPCs)
        {
            List<Frame> Results = new();

            uint[] IVs;
            uint EC;
            uint PID;
            uint Nature;
            uint AbilityRoll;
            uint FixedSeed;
            uint ShinyXOR;
            uint MockPID;
            string Gender;
            uint Height;
            bool PassIVs, Shiny;
            ulong advance = 0;
            string Jump = string.Empty;

            ulong ProgressUpdateInterval = advances / 100;
            if (ProgressUpdateInterval == 0)
                ProgressUpdateInterval++;

            Xoroshiro128Plus go = new(state0, state1);

            for (ulong i = 0; i < InitialAdvances; i++)
            {
                go.Next();
            }

            while (advance < advances)
            {
                if (progress != null && advance % ProgressUpdateInterval == 0)
                    progress.Report(1);

                // Init new RNG
                (ulong s0, ulong s1) = go.GetState();
                Xoroshiro128Plus rng = new(s0, s1);
                if (Filters.MenuClose)
                {
                    Jump = $"+{MenuClose.Generator.GetAdvances(rng, NPCs, Filters.UseWeatherFidgets, Filters.HoldingDirection)}";
                    rng = MenuClose.Generator.Advance(ref rng, NPCs, Filters.UseWeatherFidgets, Filters.HoldingDirection);
                }
                Gender = "";
                uint LeadRand = (uint)rng.NextInt(100);
                if (Filters.CuteCharm && LeadRand < 66)
                    Gender = "CC";


                Shiny = false;
                if (!Filters.ShinyLocked)
                {
                    for (int roll = 0; roll < Filters.ShinyRolls; roll++)
                    {
                        MockPID = (uint)rng.Next();
                        Shiny = Util.Common.GetTSV(Util.Common.GetTSV(MockPID >> 16, MockPID & 0xFFFF), Filters.TSV) < 16;
                        if (Shiny)
                            break;
                    }
                }

                // Gender
                if (Gender != "CC")
                    Gender = rng.NextInt(2) == 0 ? "F" : "M";

                // Nature
                Nature = (uint)rng.NextInt(25);
                if (!Util.Common.PassesNatureFilter((int)Nature, Filters.DesiredNature!))
                {
                    go.Next();
                    advance++;
                    continue;
                }
                // Ability
                AbilityRoll = 2;
                if (!Filters.AbilityLocked)
                    AbilityRoll = (uint)rng.NextInt(2);

                FixedSeed = (uint)rng.Next();
                (EC, PID, IVs, ShinyXOR, PassIVs, Height) = Util.Common.CalculateFixed(FixedSeed, Filters.TSV, Shiny, (int)Filters.FlawlessIVs, Filters.MinIVs!, Filters.MaxIVs!);
                if (Filters.Is3Segment && PID % 100 != 0)
                {
                    go.Next();
                    advance++;
                    continue;
                }

                if (!PassIVs ||
                    Filters.DesiredShiny == "Square" && ShinyXOR != 0 ||
                    Filters.DesiredShiny == "Star" && (ShinyXOR > 15 || ShinyXOR == 0) ||
                    Filters.DesiredShiny == "Star/Square" && ShinyXOR > 15 ||
                    Filters.DesiredShiny == "No" && ShinyXOR < 16
                    )
                {
                    go.Next();
                    advance++;
                    continue;
                }

                if (!Util.Common.PassesHeightFilter((int)Height, Filters.DesiredHeight!))
                {
                    go.Next();
                    advance++;
                    continue;
                }

                string Mark = Util.Common.GenerateMark(ref rng, Filters.Weather, Filters.Fishing, Filters.MarkRolls);

                if (!Util.Common.PassesMarkFilter(Mark, Filters.DesiredMark!))
                {
                    go.Next();
                    advance++;
                    continue;
                }


                // Passes all filters!
                (ulong _s0, ulong _s1) = go.GetState();
                Results.Add(
                    new Frame
                    {
                        Advances = (advance + InitialAdvances).ToString("N0"),
                        Animation = (_s0 & 1) ^ (_s1 & 1),
                        Jump = Jump,
                        PID = PID.ToString("X8"),
                        EC = EC.ToString("X8"),
                        Shiny = ShinyXOR == 0 ? "Square" : ShinyXOR < 16 ? $"Star ({ShinyXOR})" : "No",
                        Ability = AbilityRoll == 0 ? 1 : 0,
                        Nature = Util.Common.Natures[(int)Nature],
                        Gender = Gender,
                        HP = IVs[0],
                        Atk = IVs[1],
                        Def = IVs[2],
                        SpA = IVs[3],
                        SpD = IVs[4],
                        Spe = IVs[5],
                        Mark = Mark,
                        Height = Util.Common.GenerateHeightScale(Height),
                        State0 = _s0.ToString("X16"),
                        State1 = _s1.ToString("X16"),
                    }
                );
                // Cap to 2500 results
                if (Results.Count >= 2500)
                    return Results;
                go.Next();
                advance++;
            }
            return Results;
        }
    }
}
