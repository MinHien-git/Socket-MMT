using System;

namespace ConsumeJSON
{
    public class HotelRoom
    {
        int id;
        bool isBooked;
    }

    public class Hotel
    {
        int id;
        string _name;
        List<HotelRoom> rooms;

    }

    public class HotelCollection
    {

    }
}
