using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.ComponentModel;
using System.Collections;
using System.Web.Mvc;
using CST.Prdn.Data;

namespace CST.Prdn.ViewModels
{
    public enum PrdnCalDayMonth { LastMonth, ThisMonth, NextMonth };

    public class PrdnCalendarDay
    {
        private bool existsInDB;

        [DisplayName("Exists In DB")]
        public bool ExistsInDB
        {
            get { return existsInDB; }
            set { existsInDB = value; }
        }

        private PrdnCalDayMonth calMonth = PrdnCalDayMonth.ThisMonth;
        public PrdnCalDayMonth CalMonth
        {
            get { return calMonth; }
            set
            {
                calMonth = value;
            }
        }

        public void CalcCalMonth(DateTime firstDayOfMonth, DateTime lastDayOfMonth, DateTime lastRunDate)
        {
            if (CalDay < firstDayOfMonth)
            { calMonth = PrdnCalDayMonth.LastMonth; }
            else if (CalDay > lastDayOfMonth)
            { calMonth = PrdnCalDayMonth.NextMonth; }
            else
            { calMonth = PrdnCalDayMonth.ThisMonth; }

            FutureRunsExist = (CalDay.Date < lastRunDate.Date);
        }

        [DisplayName("Date")]
        public DateTime CalDay { get; set; }

        public IEnumerable<CST.Prdn.Data.ProductionOrder> ShipPrdnOrders { get; set; }

        [DisplayName("Total Runs")]
        public int TotalRuns { get; set; }

        public bool FutureRunsExist { get; set; }

        public bool ShipDay { get { return !String.IsNullOrEmpty(ShipPrdnOrdNo); } }

        public bool Editable
        {
            get
            {
                return (
                    calMonth == PrdnCalDayMonth.ThisMonth)
                    && (TotalRuns == 0)
                    && (!FutureRunsExist)
                    ;
            }
        }

        [DisplayName("PO Num")]
        public string ShipPrdnOrdNo
        {
            get
            {
                if (ShipPrdnOrders != null)
                {
                    CST.Prdn.Data.ProductionOrder shipPo = ShipPrdnOrders.FirstOrDefault();
                    if (shipPo != null)
                    { return shipPo.OrderNo; }
                }
                return String.Empty;
            }
        }

        const string format = "MMMddyyyy";

        [DisplayName("PO Num")]
        public string FormatDateID
        {
            get
            {
                return FormatString(this.CalDay);
            }
        }
        public static string FormatString(DateTime date)
        {
            return date.ToString(format, CultureInfo.InvariantCulture);
        }
        public static DateTime FormatDate(string dateString)
        {
            return DateTime.ParseExact(dateString, format, CultureInfo.InvariantCulture);
        }

        public bool Weekend()
        {
            return ((CalDay.DayOfWeek == DayOfWeek.Saturday) || (CalDay.DayOfWeek == DayOfWeek.Sunday));
        }

    }

    public class PrdnCalMonth
    {
        PrdnEntities prdnDB = null;
        DateTime maxCalDate;
        CST.Prdn.Data.ProductionOrder maxProductionOrder;
        DateTime lastPrdnOrdRunDt;

        public PrdnCalMonth(int theYear, int theMonth, PrdnEntities thePrdnDB,
            DateTime maxDate, CST.Prdn.Data.ProductionOrder maxPrdnOrder, DateTime lastRunDate)
        {
            year = theYear;
            month = theMonth;
            prdnDB = thePrdnDB;
            firstDayOfMonth = new DateTime(year, month, 1);
            firstDayOfCalendar = StartOfWeek(firstDayOfMonth, firstDayOfWeek).Date;

            int nextMonth = firstDayOfMonth.AddMonths(1).Month;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            lastDayOfMonth = new DateTime(year, month, daysInMonth).Date;
            lastDayOfCalendar = StartOfWeek(lastDayOfMonth.AddDays(7), firstDayOfWeek).AddDays(-1).Date;

            monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

            maxCalDate = maxDate;

            if (maxPrdnOrder != null)
            { maxProductionOrder = maxPrdnOrder; }
            else
            { maxProductionOrder = new CST.Prdn.Data.ProductionOrder() { ShipDay = DateTime.MinValue, OrderNo = "0" }; }

            lastPrdnOrdRunDt = lastRunDate;
        }

        int year = 0;
        public int Year { get { return year; } }
        int month = 0;
        public int Month { get { return month; } }
        string monthName = "";
        public string MonthName { get { return monthName; } }
        DayOfWeek firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
        public DayOfWeek FirstDayOfWeek { get { return firstDayOfWeek; } }
        DateTime firstDayOfMonth = DateTime.MinValue;
        public DateTime FirstDayOfMonth { get { return firstDayOfMonth; } }
        DateTime lastDayOfMonth = DateTime.MinValue;
        public DateTime LastDayOfMonth { get { return lastDayOfMonth; } }
        DateTime firstDayOfCalendar = DateTime.MinValue;
        public DateTime FirstDayOfCalendar { get { return firstDayOfCalendar; } }
        DateTime lastDayOfCalendar = DateTime.MinValue;
        public DateTime LastDayOfCalendar { get { return lastDayOfCalendar; } }

        List<PrdnCalendarDay> calendarDays = null;
        public List<PrdnCalendarDay> CalendarDays
        {
            get
            {
                if (calendarDays == null)
                {
                    calendarDays = makeCalendarDays();
                }
                return calendarDays;
            }
        }

        public IEnumerable PrdnCalWeek()
        {
            PrdnCalendarDay[] week = new PrdnCalendarDay[7];
            int i = 0;
            foreach (PrdnCalendarDay day in CalendarDays)
            {
                week[i] = day;
                if (i < 6)
                {
                    i++;
                }
                else
                {
                    yield return week;
                    i = 0;
                    week = new PrdnCalendarDay[7];
                }
            }
        }

        public IEnumerable DaysInCalendar()
        {
            DateTime day = firstDayOfCalendar;
            while (day.Date <= lastDayOfCalendar)
            {
                yield return day.Date;
                day = day.AddDays(1).Date;
            }
        }

        protected List<PrdnCalendarDay> makeCalendarDays()
        {
            var daysOfCal = from DateTime day in DaysInCalendar()
                            select day;

            // Oraclde ODP.Net generates bad sql for 10g, corelated subquery two levels deep
            //var dbCalDays = from day in prdnDB.CalendarDays
            //                  where ((day.CalDay >= firstDayOfCalendar) && (day.CalDay <= lastDayOfCalendar))
            //                  orderby day.CalDay
            //                  select new PrdnCalendarDay()
            //                  {   ExistsInDB = true,
            //                      CalDay = day.CalDay,
            //                      ShipPrdnOrders = day.ShipPrdnOrders,
            //                      TotalRuns = day.ShipPrdnOrders.Sum(p => (int?)p.Runs.Count) ?? 0};

            var dbCalDays = (from day in prdnDB.CalendarDays.Include("ShipPrdnOrders").Include("ShipPrdnOrders.Runs")
                             select day).ToList();

            // have to make PrdnCalendarDays in C# because generated Oracle code fails
            List<PrdnCalendarDay> dbPrdnCalDays = new List<PrdnCalendarDay>();

            foreach (var day in dbCalDays)
            {
                dbPrdnCalDays.Add(
                    new PrdnCalendarDay()
                    {
                        ExistsInDB = true,
                        CalDay = day.CalDay,
                        ShipPrdnOrders = day.ShipPrdnOrders,
                        TotalRuns = day.ShipPrdnOrders.Sum(p => (int?)p.Runs.Count) ?? 0
                    }
                );
            }

            var prdnCalDays = from d in daysOfCal
                              join p in dbPrdnCalDays on d equals p.CalDay into outer
                              from o in outer.DefaultIfEmpty()
                              select o ?? new PrdnCalendarDay()
                              {
                                  ExistsInDB = false,
                                  CalDay = d,
                              };

            List<PrdnCalendarDay> calendarDays = new List<PrdnCalendarDay>();

            string lastPoNo = maxProductionOrder.OrderNo;

            foreach (var prdnCalDay in prdnCalDays)
            {
                prdnCalDay.CalcCalMonth(firstDayOfMonth, lastDayOfMonth, lastPrdnOrdRunDt);

                if ((prdnCalDay.CalMonth != PrdnCalDayMonth.NextMonth)
                    && (!prdnCalDay.ExistsInDB)
                    && (prdnCalDay.CalDay > maxCalDate))  
                {
                    prdnDB.CalendarDays.AddObject(CST.Prdn.Data.CalendarDay.CreateCalendarDay(prdnCalDay.CalDay));
                    prdnCalDay.ExistsInDB = true;

                    if (((prdnCalDay.ShipPrdnOrders == null) || (prdnCalDay.ShipPrdnOrders.Count() == 0))
                        && (!prdnCalDay.Weekend())
                        && (prdnCalDay.CalDay > maxProductionOrder.ShipDay))
                    {
                        lastPoNo = CST.Prdn.Data.ProductionOrder.IncrementPrdnOrdNo(lastPoNo);
                        CST.Prdn.Data.ProductionOrder newProductionOrder =
                                CST.Prdn.Data.ProductionOrder.CreateProductionOrder(lastPoNo, prdnCalDay.CalDay);
                        prdnDB.ProductionOrders.AddObject(newProductionOrder);

                        prdnCalDay.ShipPrdnOrders = new[] { newProductionOrder };
                    }
                }
                calendarDays.Add(prdnCalDay);
            }
            prdnDB.SaveChanges();
            return calendarDays;
        }

        public static DateTime StartOfWeek(DateTime weekDay, DayOfWeek firstDayOfWeek)
        {
            //difference in days
            int diff = (int)weekDay.DayOfWeek - (int)firstDayOfWeek;

            //As a result we need to have day 0,1,2,3,4,5,6 
            if (diff < 0)
            {
                diff += 7;
            }
            return weekDay.AddDays(-1 * diff).Date;
        }
    }

    public class ProductionCalendar
    {
        PrdnCalMonth calMonth = null;

        public DateTime FirstPrdnDate { get; private set; }
        public int MinPrdnMonth { get; set; }
        public int MinPrdnYear { get; set; }
        public DateTime LastPrdnDate { get; private set; }
        public int MaxPrdnMonth { get; set; }
        public int MaxPrdnYear { get; set; }

        public CST.Prdn.Data.ProductionOrder MaxPrdnOrder { get; set; }
        public CST.Prdn.Data.ProductionOrder MaxPrdnOrderWithRun { get; set; }

        private PrdnEntities prdnDbContext;
        public PrdnEntities PrdnDBContext
        {
            get { return prdnDbContext; }
            set
            {
                prdnDbContext = value;

                MaxPrdnOrder = (from po in prdnDbContext.ProductionOrders
                                       where po.ShipDay == (from pom in prdnDbContext.ProductionOrders select pom.ShipDay).Max()
                                       select po).FirstOrDefault();

                MaxPrdnOrderWithRun = (from po in prdnDbContext.ProductionOrders
                                        where po.ShipDay == (from r in prdnDbContext.ProductionRuns
                                                            select r.PrdnOrder.ShipDay).Max()
                                       select po).FirstOrDefault();

                var days = from calDay in prdnDbContext.CalendarDays
                           group calDay by 1 into g // Notice here, grouping by a constant value
                           select new
                           {
                               MinDate = g.Min(p => p.CalDay),
                               MaxDate = g.Max(p => p.CalDay)
                           };

                var dates = days.ToList().FirstOrDefault(); // another Oracle ODP.Net bug, without ToList, both dates are first date. sigh...
                if (dates != null)
                {
                    FirstPrdnDate = dates.MinDate;
                    MinPrdnMonth = FirstPrdnDate.Month;
                    MinPrdnYear = FirstPrdnDate.Year;

                    LastPrdnDate = dates.MaxDate;

                    MaxPrdnMonth = LastPrdnDate.Month;
                    MaxPrdnYear = LastPrdnDate.Year;

                    Years = MakeYears();
                }
                else
                {
                    FirstPrdnDate = DateTime.MinValue;
                    MinPrdnMonth = 0;
                    MinPrdnYear = 0;
                    LastPrdnDate = DateTime.MinValue;
                    MaxPrdnMonth = 0;
                    MaxPrdnYear = 0;
                }
            }
        }

        public SelectList Years { get; private set; }
        public SelectList Months { get; private set; }

        private SelectList MakeYears()
        {

            var years = Enumerable.Range(MinPrdnYear, (MaxPrdnYear - MinPrdnYear) + 1).Select(x => new SelectListItem
            {
                Value = x.ToString(),
                Text = x.ToString()
            });

            return new SelectList(years, "Value", "Text");
        }

        private SelectList MakeMonths()
        {
            List<SelectListItem> months = new List<SelectListItem>();

            int minMo = 1;
            int maxMo = 12;
            if (year == MinPrdnYear)
            {
                minMo = MinPrdnMonth;
            }
            if (year == MaxPrdnYear)
            {
                maxMo = MaxPrdnMonth;
            }
            for (int i = minMo; i <= maxMo; i++)
            {
                months.Add(
                    new SelectListItem
                    {
                        Value = i.ToString(),
                        Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i)
                    });
            }

            return new SelectList(months, "Value", "Text");
        }

        private int year;
        public int Year
        {
            get { return year; }
            set { year = value; }
        }

        private int month;
        public int Month
        {
            get { return month; }
            set { month = value; }
        }

        public void AdjustYearMonth()
        {
            int yearWas = year;
            int monthWas = month;

            if (year < MinPrdnYear)
            { year = MinPrdnYear; }
            else if (year > MaxPrdnYear)
            { year = MaxPrdnYear; }

            Months = MakeMonths();

            if (year > yearWas)
            {
                month = MinPrdnMonth;
            }
            else if (year < yearWas)
            {
                month = MaxPrdnMonth;
            }
            else if ((year == MinPrdnYear) && (month < MinPrdnMonth))
            { 
                month = MinPrdnMonth; 
            }
            else if ((year == MaxPrdnYear) && (month > MaxPrdnMonth))
            { 
                month = MaxPrdnMonth; 
            }
        }

        public bool HasYearMonth()
        {
            return ((year != 0) && (month != 0));
        }

        public DateTime LDOPrevMonth { get { return new DateTime(year, month, 1).AddDays(-1); } }
        public int PrevMonth { get { return LDOPrevMonth.Month; } }
        public int PrevYear { get { return LDOPrevMonth.Year; } }
        public bool HasPrevMonth { get { return (LDOPrevMonth > FirstPrdnDate); } }

        public DateTime FDONextMonth { get { return new DateTime(year, month, DateTime.DaysInMonth(year, month)).AddDays(1); } }
        public int NextMonth { get { return FDONextMonth.Month; } }
        public int NextYear { get { return FDONextMonth.Year; } }
        public bool HasNextMonth { get { return (FDONextMonth < LastPrdnDate); } }

        public DateTime FirstUndefinedMonth
        {
            get
            {
                return new DateTime(LastPrdnDate.Year, LastPrdnDate.Month,
                    DateTime.DaysInMonth(LastPrdnDate.Year, LastPrdnDate.Month)).AddDays(1);
            }
        }

        public void MakeProductionMonth()
        {
            if (HasYearMonth())
            {
                calMonth = new PrdnCalMonth(year, month, prdnDbContext, LastPrdnDate, MaxPrdnOrder,
                    (MaxPrdnOrderWithRun != null ? MaxPrdnOrderWithRun.ShipDay : DateTime.MinValue));
            }
            else
            {
                calMonth = null;
            }
        }

        public void MakeProductionMonthDays()
        {
            calMonth = new PrdnCalMonth(year, month, prdnDbContext, LastPrdnDate, MaxPrdnOrder,
                 (MaxPrdnOrderWithRun != null ? MaxPrdnOrderWithRun.ShipDay : DateTime.MaxValue));
            List<PrdnCalendarDay> calendarDays = calMonth.CalendarDays;
        }

        public PrdnCalMonth CalMonth { get { return calMonth; } }

        public void SetShipDay(DateTime day)
        {
            DateTime nonShipDay = day.Date;
            if (MaxPrdnOrder == null)
            {
                throw new Exception("The system contains no Production Orders");
            }

            DateTime maxDate = MaxPrdnOrder.ShipDay.Date;

            if (nonShipDay == maxDate)
            {
                throw new Exception(nonShipDay.ToString("d") + " is already a ship day.");
            }
            else if (nonShipDay > maxDate)
            {
                string newProdOrdNo = CST.Prdn.Data.ProductionOrder.IncrementPrdnOrdNo(MaxPrdnOrder.OrderNo);
                CST.Prdn.Data.ProductionOrder newProdOrd = CST.Prdn.Data.ProductionOrder.CreateProductionOrder(newProdOrdNo, nonShipDay);
                prdnDbContext.AddToProductionOrders(newProdOrd);
                prdnDbContext.SaveChanges();
            }
            else // (nonShipkDay < maxDate)
            {
                ShiftPOsNext(nonShipDay, maxDate);
            }
        }

        public void ShiftPOsNext(DateTime nonShipDay, DateTime maxDate)
        {
            var calDays = (from c in prdnDbContext.CalendarDays.Include("ShipPrdnOrders")
                           where ((c.CalDay >= nonShipDay) && (c.CalDay <= maxDate))
                           orderby c.CalDay
                           select c).ToList();

            string lastProdOrdNo = String.Empty;
            CST.Prdn.Data.CalendarDay prevCalDay = calDays.FirstOrDefault();
            if (prevCalDay != null)
            {
                foreach (var nextCalDay in calDays.Skip(1))
                {
                    if ((nextCalDay.ShipPrdnOrders != null) && (nextCalDay.ShipPrdnOrders.Count() > 0))
                    {
                        foreach (var nextPo in nextCalDay.ShipPrdnOrders.ToList())
                        {
                            lastProdOrdNo = nextPo.OrderNo;
                            nextPo.ShipCalendarDay = prevCalDay;
                        }
                        prevCalDay = nextCalDay;
                    }
                }
            }

            if ((prevCalDay != null) && (!String.IsNullOrEmpty(lastProdOrdNo)))
            {
                string newPoOrdNo = CST.Prdn.Data.ProductionOrder.IncrementPrdnOrdNo(lastProdOrdNo);
                CST.Prdn.Data.ProductionOrder newProductionOrder =
                    CST.Prdn.Data.ProductionOrder.CreateProductionOrder(newPoOrdNo, prevCalDay.CalDay);
                prevCalDay.ShipPrdnOrders.Add(newProductionOrder);
            }

            prdnDbContext.SaveChanges();
        }

        public void UnSetShipDay(DateTime shipDay)
        {
            DateTime maxDate;
            if (MaxPrdnOrder != null)
            {
                maxDate = MaxPrdnOrder.ShipDay.Date;
            }
            else { throw new Exception("No shipping days are currently defined."); }

            if (shipDay > maxDate)
            { throw new Exception(shipDay.ToString("d") + " is not currently a ship day."); }

            var calDays = (from c in prdnDbContext.CalendarDays.Include("ShipPrdnOrders")
                           where ((c.CalDay >= shipDay) && (c.CalDay <= maxDate))
                           orderby c.CalDay descending
                           select c).ToList();

            CST.Prdn.Data.CalendarDay firstCalDay = calDays.LastOrDefault();
            if (firstCalDay == null)
            {
                throw new Exception("The production calendar has not be defined for " + shipDay.ToString("d") + ".");
            }
            else if (firstCalDay.ShipPrdnOrders.Count < 1)
            {
                throw new Exception(shipDay.ToString("d") + " is not currently a ship day.");
            }

            CST.Prdn.Data.CalendarDay lastCalDay = calDays.First();
            

            foreach (var lastPo in lastCalDay.ShipPrdnOrders.ToList())
            {
                lastCalDay.ShipPrdnOrders.Remove(lastPo);
                prdnDbContext.DeleteObject(lastPo);
            }

            CST.Prdn.Data.CalendarDay nextCalDay = lastCalDay;
            foreach (var prevCalDay in calDays.Skip(1))
            {
                if ((nextCalDay != null) &&
                (prevCalDay.ShipPrdnOrders != null) && (prevCalDay.ShipPrdnOrders.Count() > 0))
                {
                    foreach (var prevPo in prevCalDay.ShipPrdnOrders.ToList())
                    {
                        prevPo.ShipCalendarDay = nextCalDay;
                    }
                    nextCalDay = prevCalDay;
                }
            }
            prdnDbContext.SaveChanges();
        }

        public bool AllowEditing { get; set; }

        public string OrderController { get; set; }
        public string OrderAction { get; set; }
        
        public string RunController { get; set; }
        public string RunAction { get; set; }

        public bool DateAdjusted { get; set; }

        public static ProductionCalendar MakeProductionCalendar(string year, string month, PrdnEntities prdnDBContext)
        {
            ProductionCalendar cal = new ProductionCalendar();

            int yearInt = (year == null) ? 0 : Convert.ToInt32(year);
            int monthInt = (month == null) ? 0 : Convert.ToInt32(month);

            cal.Year = yearInt;
            cal.Month = monthInt;

            cal.PrdnDBContext = prdnDBContext;

            if ((cal.MinPrdnYear == 0) || (cal.MinPrdnMonth == 0))
            {
                throw new Exception("Production Calendar is Empty.");
            }

            if (!cal.HasYearMonth())
            {
                DateTime today = DateTime.Today;
                cal.Year = today.Year;
                cal.Month = today.Month;
            }

            cal.DateAdjusted = false;
            cal.AdjustYearMonth();

            if ((cal.Year == yearInt) && (cal.Month == monthInt))
            {
                cal.MakeProductionMonth();
            }
            else
            {
                cal.DateAdjusted = true;
            }


            return cal;
        }

    }
}