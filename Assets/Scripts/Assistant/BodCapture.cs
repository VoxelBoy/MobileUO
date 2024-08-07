using System;
using System.Collections.Generic;
using System.IO;
using ClassicUO.Configuration;

namespace Assistant.Core
{
    public class BodCapture
    {
        private class Bod
        {
            public bool IsLarge { get; set; }
            public string ItemName { get; set; }
            public bool Exceptional { get; set; }
            public string Material { get; set; }
            public string TotalAmount { get; set; }
            public string CurrentAmount { get; set; }
        }

        private static readonly uint _largeBodGumpId = 2703603018;
        private static readonly uint _smallBodGumpId = 1526454082;

        private static readonly string _bodFile = Path.GetDirectoryName(Profile.DataPath);

        public static bool IsBodGump(uint gumpId)
        {
            return gumpId == _largeBodGumpId || gumpId == _smallBodGumpId;
        }

        public static void CaptureBod(List<string> bodGumpString)
        {
            // sort the gump string
            List<Bod> bods = ParseBodGumpData(bodGumpString);

            CheckFile();

            using (StreamWriter sw = File.AppendText(_bodFile))
            {
                foreach (Bod bod in bods)
                {
                    sw.WriteLine($"{bod.ItemName},{(bod.IsLarge ? "large" : "small")},{bod.Exceptional},{bod.Material},{bod.CurrentAmount},{bod.TotalAmount}");
                }
            }
        }

        public static void CheckFile()
        {
            if (File.Exists(_bodFile))
                return;

            using (StreamWriter sw = File.AppendText(_bodFile))
            {
                sw.WriteLine("itemname,type,exceptional,material,currentamount,totalamount");
            }
        }

        /*Count = 11               //// SMALL BOD
        [0]: "A bulk order"
        [1]: "Amount to make:"
        [2]: "Amount finished:"
        [3]: "Item requested:"
        [4]: "cloak"
        [5]: "Special requirements to meet:"
        [6]: "All items must be exceptional."
        [?]: "All items must be made with X.
        [7]: "Combine this deed with the item requested."
        [8]: "EXIT"
        [9]: "20"
        [10]: "0"
        
        Count = 19                /// LARGE BOD
        [0]: "A large bulk order"
        [1]: "Amount to make:"
        [2]: "Items requested:"
        [3]: "Amount finished:"
        [4]: "war axe"
        [5]: "hammer pick"
        [6]: "mace"
        [7]: "maul"
        [8]: "war hammer"
        [9]: "war mace"
        [?]: "All items must be made with X.
        [10]: "Combine this deed with another deed."
        [11]: "EXIT"
        [12]: "15"
        [13]: "0"
        [14]: "0"
        [15]: "0"
        [16]: "0"
        [17]: "0"
        [18]: "0"*/

        private static List<Bod> ParseBodGumpData(List<string> gumpData)
        {
            List<Bod> bods = new List<Bod>();

            // First item should say large if it is
            bool isLarge = gumpData[0].Contains("large");

            // EXIT on large/small is an good "split" point
            int exitIndex = gumpData.FindIndex(x => x.Equals("EXIT"));


            // This word should exist some place in the array if it's exceptional
            bool isExceptional = false;
            string material = "default";

            foreach (string data in gumpData)
            {
                if (data.Contains("exceptional"))
                {
                    isExceptional = true;
                }
                else if (data.Contains("All items must be made with"))
                {
                    material = data.Substring(data.IndexOf("with", StringComparison.Ordinal) + 5).Replace(".", "");
                }
            }

            // Based on the data above, the total amount is always after EXIT in both small and large
            string totalAmount = gumpData[exitIndex + 1];

            // BOD requirement data appears at index 4 for both small and large
            int beginningIndex = 4;

            int currentAmountIndex = 2;

            if (isLarge)
            {
                for (int i = beginningIndex; i < gumpData.Count; i++)
                {
                    // Keep adding new BODs to the array as long as you don't hit the end
                    if (!gumpData[i].Contains("Combine") && !gumpData[i].Contains("Special"))
                    {
                        bods.Add(new Bod
                        {
                            ItemName = gumpData[i],
                            IsLarge = true,
                            Exceptional = isExceptional,
                            CurrentAmount = gumpData[exitIndex + currentAmountIndex],
                            TotalAmount = totalAmount,
                            Material = material
                        });

                        currentAmountIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else // small!
            {
                bods.Add(new Bod
                {
                    ItemName = gumpData[beginningIndex],
                    IsLarge = false,
                    Exceptional = isExceptional,
                    CurrentAmount = gumpData[exitIndex + 2],
                    TotalAmount = totalAmount,
                    Material = material
                });
            }

            return bods;
        }

    }
}
