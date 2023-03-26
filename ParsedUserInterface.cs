//using System;
//using System.Runtime.InteropServices;
//using PrimaryScreen;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//namespace ConsoleApp
//{
//    class Program
//    {
//        private const string DisplayXKey = "_displayX";
//        private const string DisplayYKey = "_displayY";
//        private const string TextKey = "_text";
//        private const string DisplayWidthKey = "_displayWidth";
//        private const string DisplayHeightKey = "_displayHeight";
//        private const string ChildrenKey = "children";
//        private const string DictEntriesOfInterestKey = "dictEntriesOfInterest";

//        public static bool Read(JObject jsonData, int x, int y)
//        {
//            if (!jsonData.ContainsKey(DictEntriesOfInterestKey)) return false;
//            var dictEntriesOfInterest = jsonData[DictEntriesOfInterestKey].ToObject<JObject>();

//            if (dictEntriesOfInterest.ContainsKey(DisplayXKey) && dictEntriesOfInterest[DisplayXKey] is not JObject)
//                x += dictEntriesOfInterest[DisplayXKey].Value<int>();

//            if (dictEntriesOfInterest.ContainsKey(DisplayYKey) && dictEntriesOfInterest[DisplayYKey] is not JObject)
//                y += dictEntriesOfInterest[DisplayYKey].Value<int>();


//            //if ("SpaceObjectIcon" == jsonData["pythonObjectTypeName"].Value<string>())
//            //{
//            //    if (jsonData.ContainsKey("children") && jsonData["children"][0]["dictEntriesOfInterest"].ToObject<JObject>().ContainsKey("_color"))
//            //    {
//            //        var argb = jsonData["children"][0]["dictEntriesOfInterest"]["_color"];
//            //        if (argb["aPercent"].Value<int>() == 100 && argb["rPercent"].Value<int>() == 100 && argb["gPercent"].Value<int>() == 100 && argb["bPercent"].Value<int>() == 0)
//            //        {
//            //            return true;
//            //        }
//            //    }
//            //}

//            var key = "Tannakan";
//            if (dictEntriesOfInterest.ContainsKey(TextKey) && dictEntriesOfInterest[TextKey].Value<string>().Contains(key))
//                return true;


//            if (!jsonData.ContainsKey(ChildrenKey)) return false;
//            foreach (var child in jsonData[ChildrenKey])
//            {
//                    if (Read(child.ToObject<JObject>(), x, y))
//                    {
//                        x += dictEntriesOfInterest[DisplayWidthKey].Value<int>() / 2;
//                        y += dictEntriesOfInterest[DisplayHeightKey].Value<int>() / 2;
//                        Console.WriteLine("{0},{1}", x, y);
//                        break;
//                    }
//                }
//            return false;
//        }

//        static void Main(string[] args)
//        {
//            var fileName = @"D:\code\eveonline-robot\main\bin\Debug\net7.0\111111111111111.json";
//            JObject jsonData = JObject.Parse(File.ReadAllText(fileName));
//            int x = 0, y = 0;
//            Read(jsonData, x,y);
//        }
//    }
//}
