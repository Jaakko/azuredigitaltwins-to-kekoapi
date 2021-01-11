namespace KEKO
{
    public class Sensor
    {
        public string resource_id { get; set; }

        public string type { get; set; }

        public string area_id { get; set; }

        public string building_id { get; set; }
    }

    public class HumiditySensor
    {
        public string name { get; set; }

        public string resource_id { get; set; }

        public string humidity { get; set; }

        public string timestamp { get; set; }

    }
}