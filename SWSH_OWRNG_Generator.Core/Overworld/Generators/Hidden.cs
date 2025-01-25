using PKHeX.Core;
using static SWSH_OWRNG_Generator.Core.Overworld.Generators.Static;

namespace SWSH_OWRNG_Generator.Core.Overworld.Generators
{
    public static class Hidden
    {
        public static List<Frame> Generate(ulong state0, ulong state1, ulong advances, ulong InitialAdvances, IProgress<int> progress, Filter Filters, uint NPCs, uint rain_ticks, RainType rain_type, uint type_pull_slots = 1)
        {
            List<Frame> Results = new();

            uint[] IVs;
            bool GenerateLevel = Filters.LevelMin != Filters.LevelMax;
            uint LevelDelta = Filters.LevelMax - Filters.LevelMin + 1;
            uint EC;
            uint PID;
            string SlotRand;
            uint Level;
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

            byte[] enc = { 44, 88, 100, 100 };

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
                byte steps = 0;
                if (progress != null && advance % ProgressUpdateInterval == 0)
                    progress.Report(1);

                // Init new RNG
                (ulong s0, ulong s1) = go.GetState();
                Xoroshiro128Plus rng = new(s0, s1);

                Jump = $"+{MenuClose.Generator.GetAdvances(rng, NPCs, Filters.UseWeatherFidgets, Filters.HoldingDirection)}";

                // Exiting Pokemon menu.
                if (rain_type != RainType.None)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        rng.NextInt(20001);
                        rng.NextInt(20001);
                    }
                    // Do 3 additional in Thunderstorm.
                    if (rain_type == RainType.Thunderstorm)
                    {
                        for (var i = 0; i < 3; i++)
                        {
                            rng.NextInt(20001);
                            rng.NextInt(20001);
                        }
                    }
                }

                // Menu close.
                if (rain_type != RainType.None)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        rng.NextInt(20001);
                        rng.NextInt(20001);
                    }
                    // Do 2 additional in Thunderstorm.
                    if (rain_type == RainType.Thunderstorm)
                    {
                        for (var i = 0; i < 2; i++)
                        {
                            rng.NextInt(20001);
                            rng.NextInt(20001);
                        }
                    }
                }

                rng = MenuClose.Generator.Advance(ref rng, NPCs, Filters.UseWeatherFidgets, Filters.HoldingDirection);

                // Second batch of rain ticks. This is the same regardless of rain type.
                if (rain_type != RainType.None)
                {
                    for (var i = 0; i < rain_ticks; i++)
                    {
                        rng.NextInt(20001);
                        rng.NextInt(20001);
                    }
                }

                uint LeadRand = 0;
                for (var step_count = 0; step_count < 5; step_count++)
                {
                    steps = (byte)(step_count + 1);
                    LeadRand = (uint)rng.NextInt(100);

                    var step_rand = (uint)rng.NextInt(100);
                    if (step_rand >= enc[step_count])
                    {
                        // Rain ticks before trying again.
                        if (rain_type != RainType.None)
                        {
                            for (var i = 0; i < 3; i++)
                            {
                                rng.NextInt(20001);
                                rng.NextInt(20001);
                            }
                            // Do 3 additional in Thunderstorm.
                            if (rain_type == RainType.Thunderstorm)
                            {
                                for (var i = 0; i < 3; i++)
                                {
                                    rng.NextInt(20001);
                                    rng.NextInt(20001);
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                SlotRand = "";
                if (Filters.CuteCharm && LeadRand >= 49)
                {
                    SlotRand = "T";
                    if (type_pull_slots > 1)
                    {
                        var type_slot = (int)rng.NextInt(type_pull_slots) + 1;
                        SlotRand += type_slot;
                    }
                }

                if (SlotRand.Length == 0)
                {
                    // Attempt Dex Rec; we don't handle if it's active yet :(
                    var dexrec_rand = rng.NextInt(100);
                    if (dexrec_rand < 50)
                    {
                        // Don't want to fully implement dex rec.
                        // This assumes only slot 4 is eligible, everything else goes to normal slots.
                        var dexrec_slot = (int)rng.NextInt(4);
                        if (dexrec_slot == 3)
                            SlotRand = $"DR{dexrec_slot + 1}";
                    }
                }

                if (SlotRand.Length == 0)
                {
                    var slotrandval = (uint)rng.NextInt(100);
                    SlotRand = slotrandval.ToString();
                    if (Filters.SlotMin > slotrandval || Filters.SlotMax < slotrandval)
                    {
                        go.Next();
                        advance++;
                        continue;
                    }
                }

                if (GenerateLevel)
                {
                    Level = Filters.LevelMin + (uint)rng.NextInt(LevelDelta);
                }
                else
                {
                    Level = Filters.LevelMin;
                }

                Util.Common.GenerateMark(ref rng, Filters.Weather, Filters.Fishing, Filters.MarkRolls); // Double Mark Gen happens always

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

                Gender = "";
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

                // Held Item
                if (Filters.HeldItem)
                    rng.NextInt(100);

                FixedSeed = (uint)rng.Next();
                (EC, PID, IVs, ShinyXOR, PassIVs, Height) = Util.Common.CalculateFixed(FixedSeed, Filters.TSV, Shiny, (int)Filters.FlawlessIVs, Filters.MinIVs!, Filters.MaxIVs!);
                if (Filters.Is3Segment && EC % 100 != 0)
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
                        Animation = _s0 & 1 ^ _s1 & 1,
                        Jump = Jump,
                        Steps = steps.ToString(),
                        Level = Level,
                        Slot = SlotRand,
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
