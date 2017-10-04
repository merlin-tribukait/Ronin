using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Data
{
    public static class ExportedData
    {
        public static Dictionary<int, string> SkillsIdToName = new Dictionary<int, string>();
        public static Dictionary<int, string> ItemIdToName = new Dictionary<int, string>();
        public static Dictionary<int, string> NpcIdToName = new Dictionary<int, string>();
        public static Dictionary<int, Dictionary<long, long>> SkillManaConsumption = new Dictionary<int, Dictionary<long, long>>();
        public static Dictionary<string, List<string>> TownTeleportRelations = new Dictionary<string, List<string>>();
        public static Dictionary<int, long> SkillIdToMaxDistance = new Dictionary<int, long>();
        public static List<int> BowItemIds = new List<int>();
        public static List<int> CrossBowItemIds = new List<int>();

        public static void Init()
        {
            string[] lines = System.IO.File.ReadAllLines(@"Resources\skillname-e.txt");

            for (int i = 1; i < lines.Length; i++)
            {
                var split = lines[i].Split(new string[] { "\t" }, StringSplitOptions.None);
                var id = int.Parse(split[0]);
                var name = split[2].Replace("a,", String.Empty).Replace("\\0", String.Empty);
                if (SkillsIdToName.ContainsKey(id) == false)
                    SkillsIdToName.Add(id, name);
            }

            lines = System.IO.File.ReadAllLines(@"Resources\skillname-eIL.txt");

            for (int i = 1; i < lines.Length; i++)
            {
                var split = lines[i].Split(new string[] { "\t" }, StringSplitOptions.None);
                var id = int.Parse(split[0]);
                var name = split[2].Replace("a,", String.Empty).Replace("\\0", String.Empty);
                if (SkillsIdToName.ContainsKey(id) == false)
                    SkillsIdToName.Add(id, name);
            }

            lines = System.IO.File.ReadAllLines(@"Resources\skillgrp.txt");

            for (int i = 1; i < lines.Length; i++)
            {
                var split = lines[i].Split(new string[] { "\t" }, StringSplitOptions.None);
                var id = int.Parse(split[0]);
                var level = long.Parse(split[1]);
                var mpConsume = long.Parse(split[4]);
                var maxRange = long.Parse(split[5]);

                if (SkillManaConsumption.ContainsKey(id) == false)
                    SkillManaConsumption.Add(id, new Dictionary<long, long>());

                if (!SkillManaConsumption[id].ContainsKey(level))
                    SkillManaConsumption[id].Add(level, mpConsume);

                if (!SkillIdToMaxDistance.ContainsKey(id))
                    SkillIdToMaxDistance.Add(id,maxRange);
            }

            lines = System.IO.File.ReadAllLines(@"Resources\ItemName-e.txt");

            for (int i = 1; i < lines.Length; i++)
            {
                var split = lines[i].Split(new string[] { "\t" }, StringSplitOptions.None);
                var id = int.Parse(split[0]);
                var name = split[1];
                //name = Char.ToUpper(name[0]) + name.Substring(1);
                if (ItemIdToName.ContainsKey(id) == false)
                    ItemIdToName.Add(id, name);
            }

            lines = System.IO.File.ReadAllLines(@"Resources\npcname-e.txt");

            for (int i = 1; i < lines.Length; i++)
            {
                var split = lines[i].Split(new string[] { "\t" }, StringSplitOptions.None);
                var id = int.Parse(split[0]);
                var name = split[1].Replace("a,", String.Empty).Replace("\\0", String.Empty);
                if (NpcIdToName.ContainsKey(id) == false)
                    NpcIdToName.Add(id, name);
            }

            lines = System.IO.File.ReadAllLines(@"Resources\Weapongrp.txt");

            for (int i = 1; i < lines.Length; i++)
            {
                var split = lines[i].Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                var id = int.Parse(split[1]);
                var var1 = split[25];
                var var2 = split[26];
                var var3 = split[27];
                if (var1 == "7" && var2 == "5" && var3 == "1" && !BowItemIds.Contains(id))
                    BowItemIds.Add(id);
                else if (var1 == "7" && var2 == "8" && var3 == "1" && !CrossBowItemIds.Contains(id))
                    CrossBowItemIds.Add(id);
            }
        }
    }
}
