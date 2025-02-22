﻿using PKHeX.Core;

namespace SWSH_OWRNG_Generator.Core.Overworld.Generators
{
    public static class Symbol
    {
        public static List<Frame> Generate(ulong state0, ulong state1, ulong advances, ulong InitialAdvances, IProgress<int> progress, Filter Filters, uint NPCs, uint type_pull_slots = 1)
        {
            List<Frame> Results = new();

            uint[] IVs;
            bool GenerateLevel = Filters.LevelMin != Filters.LevelMax;
            uint LevelDelta = Filters.LevelMax - Filters.LevelMin + 1;
            uint EC;
            uint PID;
            string SlotRand;
            uint Level;
            uint BrilliantRand;
            uint Nature;
            uint AbilityRoll;
            uint FixedSeed;
            uint ShinyXOR;
            uint MockPID;
            uint BrilliantThreshold;
            uint BrilliantRolls;
            int BrilliantIVs;
            string Gender;
            uint Height;
            bool PassIVs, Brilliant, Shiny;
            ulong advance = 0;
            string Jump = string.Empty;

            (BrilliantThreshold, BrilliantRolls) = Util.Common.GenerateBrilliantInfo(Filters.KOs);

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
                Brilliant = false;

                rng.NextInt(361); // placement roll -- assuming it works on the first try.
                rng.Next(); // actually a float but we don't care about the value.


                uint LeadRand = (uint)rng.NextInt(100);
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
                        // Will only do this if you have any dex recs.
                        //var dexrec_slot = (int)rng.NextInt(4);
                        // todo stuff with with the slot.
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

                BrilliantRand = (uint)rng.NextInt(1000);
                if (BrilliantRand < BrilliantThreshold)
                {
                    Brilliant = true;
                    Level = Filters.LevelMax;
                }
                if (Filters.DesiredAura == "Brilliant" && !Brilliant || Filters.DesiredAura == "None" && Brilliant)
                {
                    go.Next();
                    advance++;
                    continue;
                }

                Shiny = false;
                if (!Filters.ShinyLocked)
                {
                    for (int roll = 0; roll < Filters.ShinyRolls + (Brilliant ? BrilliantRolls : 0); roll++)
                    {
                        MockPID = (uint)rng.Next();
                        Shiny = Util.Common.GetTSV(Util.Common.GetTSV(MockPID >> 16, MockPID & 0xFFFF), Filters.TSV) < 16;
                        if (Shiny)
                            break;
                    }
                }

                // Gender
                Gender = "";
                if (Gender != "CC")
                    Gender += rng.NextInt(2) == 0 ? "F" : "M";
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

                BrilliantIVs = 0;
                if (Brilliant)
                {
                    // Brilliant IVs
                    BrilliantIVs = (int)rng.NextInt(2) | 2;
                    // Brilliant Egg Move
                    if (Filters.EggMoveCount > 1)
                        rng.NextInt(Filters.EggMoveCount);
                }

                FixedSeed = (uint)rng.Next();
                (EC, PID, IVs, ShinyXOR, PassIVs, Height) = Util.Common.CalculateFixed(FixedSeed, Filters.TSV, Shiny, (int)(Filters.FlawlessIVs + BrilliantIVs), Filters.MinIVs!, Filters.MaxIVs!);
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
                        Level = Level,
                        Slot = SlotRand,
                        PID = PID.ToString("X8"),
                        EC = EC.ToString("X8"),
                        Shiny = ShinyXOR == 0 ? "Square" : ShinyXOR < 16 ? $"Star ({ShinyXOR})" : "No",
                        Brilliant = Brilliant ? "Y" : "-",
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
