using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Exchange.WebServices.Data;


namespace RoomChecker
{
    class Program
    {
        public const int DefaultMinimumPeople = 1;
        public const int DefaultMeetingDuration = 30;
        public const int DefaultPreferredFloor = 15;

        // *****************************************
        static string email = Cred.e; 
        static string password = Cred.p;
        // *****************************************

        static void Main(string[] args)
        {
            ExchangeService service = new ExchangeService();
            service.Credentials = new NetworkCredential(email, password);

            // Look up the user's EWS endpoint by using Autodiscover.
            service.AutodiscoverUrl(email, RedirectionCallback);

            Console.WriteLine("EWS Endpoint: {0}", service.Url);

            var rooms = service.GetRooms("Slalom--SeattleConferenceRooms@slalom.com");


            //Appointment appointment = new Appointment(service);

            //// Set the properties on the appointment object to create the appointment.
            //appointment.Subject = "Tennis lesson";
            //appointment.Body = "Focus on backhand this week.";
            //appointment.Start = DateTime.Now;
            //appointment.End = appointment.Start.AddHours(1);
            //appointment.Location = "Tennis club";
            //appointment.ReminderDueBy = DateTime.Now;

            //// Save the appointment to your calendar.
            //appointment.Save(SendInvitationsMode.SendToNone);

            //// Verify that the appointment was created by using the appointment's item ID.
            //Item item = Item.Bind(service, appointment.Id, new PropertySet(ItemSchema.Subject));
            //Console.WriteLine("\nAppointment created: " + item.Subject + "\n");

            Get(service, DefaultMinimumPeople, DefaultMeetingDuration, DefaultPreferredFloor);



        }

        static bool RedirectionCallback(string url)
        {
            return url.ToLower().StartsWith("https://");
        }

        public static void Get(
            ExchangeService service,
           int minimumPeople = DefaultMinimumPeople,
           int duration = DefaultMeetingDuration,
           int preferredFloor = DefaultPreferredFloor,
            String startTime = null)
          
        {
            try
            {
                DateTime currentDateTime = DateTime.Now.ToLocalTime();
                DateTime startDate = currentDateTime;

                if (!String.IsNullOrEmpty(startTime))
                {
                    // startTime will be in the form of = 04:00 or 14:00 (military time)
                    var startTimeChunk = startTime.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    // set the hour and minute part while keeping the date to current date
                    TimeSpan hourMinutes = new TimeSpan(int.Parse(startTimeChunk[0]), int.Parse(startTimeChunk[1]), 0);
                    startDate = startDate.Date + hourMinutes;

                    // verify that startDate is NOT in the past. Huddle API is only allowing to request room for current day, 
                    // users are allowed to request rooms sometime in the future as long as it is still today.
                    if (startDate < currentDateTime)
                    {
                        return;
                    }
                }

                DateTime endDate = DurationAdjuster.ExtendDurationToNearestBlock(startDate, duration);

                // Initailize a room loader with the service and service account.
                RoomLoader roomLoader = new RoomLoader(service, email);

                // Use the service to load all of the rooms
                List<Room> rooms = roomLoader.LoadRooms(preferredFloor);

                // Use the service to load the schedule of all the rooms
                roomLoader.LoadRoomSchedule(rooms, startDate, endDate);

                // Find the first available room that supports the number of people.
                Room selectedRoom = rooms.FirstOrDefault(n => n.Available && n.RoomInfo.MaxPeople >= minimumPeople);
                if (selectedRoom == null)
                {
                    Console.WriteLine("could not find a room");
                }

                // Acquire the meeting room for the duration.
                //Appointment meeting = RoomLoader.AcquireMeetingRoom(selectedRoom, startDate, endDate, preferredFloor, command);

                // Verify that the meeting was created by matching the subject.

                // Return a 200

                //return Ok(new { Text = RoomLoader.Wrap(meeting.Body, SuccessAudioUrl) });
                Console.WriteLine("found a room");
            }
            catch (Exception exception)
            {
                Console.WriteLine("found an exception" + exception);
            }
        }
    }
}
