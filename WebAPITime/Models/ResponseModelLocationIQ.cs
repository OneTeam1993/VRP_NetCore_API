namespace WebAPI
{
    //public class ResponseModelLocationIQ
    //{
    //    public string code { get; set; }
    //    public List<WaypointModel> waypoints { get; set; }
    //    public List<RoutesModel> routes { get; set; }
    //}

    //public class WaypointModel
    //{
    //    public string hint { get; set; }
    //    public long distance { get; set; }
    //    public string name { get; set; }

    //}

    //public class RoutesModel
    //{
    //    public List<LegsModel> legs { get; set; }
    //    public string weight_name { get; set; }
    //    public string geometry { get; set; }
    //    public long distance { get; set; }
    //    public double duration { get; set; }
    //    public double weight { get; set; }
    //}

    //public class LegsModel
    //{
    //    public double weight { get; set; }
    //    public long distance { get; set; }
    //    public string summary { get; set; }
    //    public long duration { get; set; }
    //}

    public class ResponseModelLocationIQ
    {
        public string Code { get; set; }
        public long[][] Distances { get; set; }
        public long[][] Durations { get; set; }
        public LocationInfo[] Sources { get; set; }
        public LocationInfo[] Destinations { get; set; }
    }

    public class LocationInfo
    {
        public string Hint { get; set; }
        public long Distance { get; set; }
        public double[] Location { get; set; }
        public string Name { get; set; }
    }
}
