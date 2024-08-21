using System;

public partial class Trial_DateUTC : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        TestDateTimeOffset();
        
        DateLocal();
        ConvertTimeFromUtc();
        
        TestIsInvalidTime();

    }
    
    /// <summary>
    /// Obtenir une heure utc en heure locale
    /// </summary>
    private static void ConvertTimeFromUtc()
    {
        _ = new DateTime(2002, 05, 25, 13, 20, 59);  //Heure d'été (selon Central European Standard Time)
        DateTime timeUtc = new DateTime(2002, 02, 25, 13, 20, 59);  //Heure d'hiver (selon Central European Standard Time)

        try
        {
           
            
            TimeZoneInfo cstZone = TimeZoneInfo.Local; //(UTC+01:00) Bruxelles, Copenhague, Madrid, Paris

            

            cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"); //(UTC+01:00) Sarajevo, Skopje, Varsovie, Zagreb

            DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);
            Console.WriteLine("The date and time are {0} {1}.",
                              cstTime,
                              cstZone.IsDaylightSavingTime(cstTime) ?
                                      cstZone.DaylightName : cstZone.StandardName);
        }
        catch (TimeZoneNotFoundException)
        {
            Console.WriteLine("The registry does not define the Central Standard Time zone.");
        }
        catch (InvalidTimeZoneException)
        {
            Console.WriteLine("Registry data on the Central STandard Time zone has been corrupted.");
        }


    }

    /// <summary>
    /// Obtenir une heure locale avec le fuseau horaire (TimeZoneDesignation) ex 2014-12-27T10:46:09.080+02:00
    /// </summary>
    private static void DateLocal()
    {
        //voir aussi http://www.timeanddate.com/time/zones/cet et http://www.timeanddate.com/time/zones/cest

        _ = TimeZoneInfo.Local; //(UTC+01:00) Bruxelles, Copenhague, Madrid, Paris
        TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"); //(UTC+01:00) Sarajevo, Skopje, Varsovie, Zagreb  (EUREX)

        int hours = cstZone.BaseUtcOffset.Hours;
        //en utilisant cstZone.GetUtcOffset() on obient +01:00(hiver) ou +02:00 (été) => Peut-être faut-il utiliser cette méthode

        string offset = string.Format("{0}{1}", ((hours > 0) ? "+" : ""), hours.ToString("00"));
        string isoformat = DateTime.Now.ToString("s") + offset;
        Console.WriteLine(isoformat); // Avec ce code on obtient tjs +01:00

    }

    /// <summary>
    /// vérifie que la date est valid (au passade de l'heure dété la plage entre 2h00 et 3h00 est impossible)
    /// </summary>
    private static void TestIsInvalidTime()
    {

        //passage heure d'été 2016 => 27/03/2016
        //passage heure d'hiver 2016 => 30/10/2016
        DateTime timetest2 = new DateTime(2016, 03, 27, 02, 20, 25, DateTimeKind.Unspecified);//Heure impossible
        TimeZoneInfo cstZone2 = TimeZoneInfo.Local;
        _ = cstZone2.IsInvalidTime(timetest2);

        //passage heure d'été 2017 => 26/03/2016
        //passage heure d'hiver 2017 => 29/10/2016
        DateTime timetest3 = new DateTime(2017, 03, 26, 02, 20, 25, DateTimeKind.Unspecified);//Heure impossible
        TimeZoneInfo cstZone3 = TimeZoneInfo.Local;
        _ = cstZone3.IsInvalidTime(timetest3);

    }


    /// <summary>
    /// 
    /// </summary>
    private void TestDateTimeOffset()
    {
        TimeSpan offset = new TimeSpan(-2, 0, 0);

        DateTimeOffset dtOffset = new DateTimeOffset(2002, 05, 25, 13, 20, 59, offset); //Heure d'été
        _ = dtOffset.ToString("zzz");
        _ = dtOffset.UtcDateTime;  //=> on obtient 11h20 (décalage de 2 heures)

        DateTimeOffset dtOffset2 = new DateTimeOffset(2002, 02, 25, 13, 20, 59, offset); //Heure d'hiver
        _ = dtOffset2.ToString("zzz");
        _ = dtOffset2.UtcDateTime; //=> on obtient 11h20 (décalage de 2 heures)

    }
}
