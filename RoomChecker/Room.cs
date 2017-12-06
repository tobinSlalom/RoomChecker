using Microsoft.Exchange.WebServices.Data;


namespace RoomChecker
{
    public class Room
    {
        public RoomInfo RoomInfo { get; set; }

        public AttendeeInfo AttendeeInfo { get; set; }

        public bool Available { get; set; }

        public override string ToString()
        {
            return RoomInfo.ToString();
        }
    }
}
