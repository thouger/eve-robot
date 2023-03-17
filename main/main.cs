using Newtonsoft.Json.Linq;

namespace main;
public class main
{
    public static void Main(string[] args)
    {
        //(int processId, long windowId, string windowsTitle) = Program.ListGameClientProcessesRequest();
        //ulong uiRootAddress = Program.SearchUIRootAddress(processId);
        //Console.WriteLine(uiRootAddress);
        //var getImageData = new GetImageDataFromReadingStructure
        //{
        //    screenshot1x1Rects = new Rect2d[5]
        //};
        //var memoryReadingSerialRepresentationJson = Program.ReadFromWindow(windowId, uiRootAddress, getImageData, processId);
        //File.WriteAllText("1.txt", memoryReadingSerialRepresentationJson);

        //var memoryReadingSerialRepresentationJson = File.ReadAllText("1.txt");
        var memoryReadingSerialRepresentationJson = File.ReadAllText("memory-reading.json");
        JObject json = JObject.Parse(memoryReadingSerialRepresentationJson);
        PrintJson(json);
        Console.WriteLine();
    }

    //查找包含Kedama
    public static void PrintJson(JObject token, string indent = "", int depth = 0, int x = 0, int y = 0, int _displayX = 0, int _displayY = 0)
    {


        foreach (var item in token)
        {
            var key = item.Key;
            var value = item.Value;

            try
            {
                x += (int)value["_displayX"];
               
                y += (int)value["_displayY"];
                //Console.WriteLine($"{x},{y}");
            }
            catch (Exception) { }

            if (value.Type is JTokenType.Object)
            {
                foreach (var i in (JObject)value)
                {


                    var _key = i.Key;
                    var _value = i.Value;
                    if (_value.Type is JTokenType.String && ((string)_value).Contains("Oisio III - Moon 1 - Hyasyoda Corporation Refinery<br>96 km"))
                    {

                        //x -= _displayX;
                        //y -= _displayY;
                        Console.WriteLine(_value);
                        Console.WriteLine($"{x},{y}");
                    }
                }
            }

            if (key == "children")
            {
                foreach (var i in value)
                {
                    try
                    {
                        _displayX = (int)value["_displayX"];
                        _displayY = (int)value["_displayY"];
                    }
                    catch (Exception) { }
                    PrintJson(token: i as JObject, x: x, y: y, _displayX: _displayX, _displayY: _displayY);
                }
            }
        }
    }
}