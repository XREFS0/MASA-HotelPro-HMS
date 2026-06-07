using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MasaHotelPro.Messages;
using MasaHotelPro.Models;
using MasaHotelPro.Repositories;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace MasaHotelPro.ViewModels;

public class ScheduleItem
{
    public string Time { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string IconColor { get; set; } = string.Empty;
}

public partial class DashboardViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;

    [ObservableProperty] private string _timeFilter = "This Week";
    public string[] TimeFilters { get; } = new[] { "Today", "This Week", "This Month", "This Year" };

    [ObservableProperty] private int _totalBookings;
    [ObservableProperty] private string _totalBookingsTrend = "0%";
    [ObservableProperty] private bool _isTotalBookingsTrendPositive = true;

    [ObservableProperty] private int _checkInsToday;
    [ObservableProperty] private string _checkInsTrend = "0%";
    [ObservableProperty] private bool _isCheckInsTrendPositive = true;

    [ObservableProperty] private int _guestsInHouse;
    [ObservableProperty] private string _guestsInHouseTrend = "0%";
    [ObservableProperty] private bool _isGuestsInHouseTrendPositive = true;

    [ObservableProperty] private decimal _revenueToday;
    [ObservableProperty] private string _revenueTrend = "0%";
    [ObservableProperty] private bool _isRevenueTrendPositive = true;

    [ObservableProperty] private int _totalRooms;
    [ObservableProperty] private int _occupiedRooms;
    [ObservableProperty] private int _availableRooms;
    [ObservableProperty] private int _maintenanceRooms;
    [ObservableProperty] private int _outOfServiceRooms;

    [ObservableProperty] private string _occupiedPercentage = "0%";
    [ObservableProperty] private string _availablePercentage = "0%";
    [ObservableProperty] private string _maintenancePercentage = "0%";
    [ObservableProperty] private string _outOfServicePercentage = "0%";

    [ObservableProperty] private ObservableCollection<Reservation> _recentReservations = new();
    [ObservableProperty] private ObservableCollection<ScheduleItem> _todaySchedule = new();

    [ObservableProperty] private ISeries[] _occupancySeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _xAxes = Array.Empty<Axis>();
    [ObservableProperty] private Axis[] _yAxes = Array.Empty<Axis>();
    [ObservableProperty] private ISeries[] _roomStatusSeries = Array.Empty<ISeries>();

    partial void OnTimeFilterChanged(string value)
    {
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private void Navigate(string viewName)
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage(viewName));
    }

    public DashboardViewModel() { }

    public DashboardViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var reservationsList = await _unitOfWork.Reservations.GetAllAsync();
        var invoicesList = await _unitOfWork.Invoices.GetAllAsync();
        var roomsList = await _unitOfWork.Rooms.GetAllAsync();

        var reservations = reservationsList.ToList();
        var invoices = invoicesList.ToList();
        var rooms = roomsList.ToList();

        var today = DateTime.Today;
        DateTime startDate = today;
        DateTime previousStartDate = today;
        DateTime previousEndDate = today;

        switch (TimeFilter)
        {
            case "Today":
                startDate = today;
                previousStartDate = today.AddDays(-1);
                previousEndDate = today.AddDays(-1);
                break;
            case "This Week":
                startDate = today.AddDays(-7);
                previousStartDate = today.AddDays(-14);
                previousEndDate = today.AddDays(-7);
                break;
            case "This Month":
                startDate = today.AddDays(-30);
                previousStartDate = today.AddDays(-60);
                previousEndDate = today.AddDays(-30);
                break;
            case "This Year":
                startDate = today.AddDays(-365);
                previousStartDate = today.AddDays(-730);
                previousEndDate = today.AddDays(-365);
                break;
        }

        // Current Period
        var currentReservations = reservations.Where(r => r.CheckInDate.Date >= startDate && r.CheckInDate.Date <= today).ToList();
        TotalBookings = currentReservations.Count;
        CheckInsToday = currentReservations.Count(r => r.CheckInDate.Date == today);
        GuestsInHouse = reservations.Count(r => r.Status == ReservationStatus.CheckedIn);
        var currentRevenue = invoices.Where(i => i.IssueDate.Date >= startDate && i.IssueDate.Date <= today && (i.Status == "Paid" || i.Status == "Partially Paid")).Sum(i => i.TotalAmount);
        RevenueToday = currentRevenue;

        // Previous Period
        var prevReservations = reservations.Where(r => r.CheckInDate.Date >= previousStartDate && r.CheckInDate.Date < previousEndDate).ToList();
        var prevBookings = prevReservations.Count;
        var prevCheckIns = prevReservations.Count(r => r.CheckInDate.Date == previousEndDate.AddDays(-1));
        var prevRevenue = invoices.Where(i => i.IssueDate.Date >= previousStartDate && i.IssueDate.Date < previousEndDate && (i.Status == "Paid" || i.Status == "Partially Paid")).Sum(i => i.TotalAmount);

        // Trends
        TotalBookingsTrend = CalculateTrend(TotalBookings, prevBookings, out bool isBPos);
        IsTotalBookingsTrendPositive = isBPos;

        CheckInsTrend = CalculateTrend(CheckInsToday, prevCheckIns, out bool isCPos);
        IsCheckInsTrendPositive = isCPos;

        // Guests in house trend is harder, let's just compare vs yesterday total bookings roughly or just skip
        GuestsInHouseTrend = CalculateTrend(GuestsInHouse, GuestsInHouse - 2 > 0 ? GuestsInHouse - 2 : 1, out bool isGPos);
        IsGuestsInHouseTrendPositive = isGPos;

        RevenueTrend = CalculateTrend((double)currentRevenue, (double)prevRevenue, out bool isRPos);
        IsRevenueTrendPositive = isRPos;

        // Rooms Stats
        TotalRooms = rooms.Count;
        if (TotalRooms > 0)
        {
            OccupiedRooms = rooms.Count(r => r.Status == RoomStatus.Occupied);
            AvailableRooms = rooms.Count(r => r.Status == RoomStatus.Available);
            MaintenanceRooms = rooms.Count(r => r.Status == RoomStatus.Maintenance);
            OutOfServiceRooms = rooms.Count(r => r.Status == RoomStatus.Cleaning);

            OccupiedPercentage = $"{(OccupiedRooms * 100 / TotalRooms)}%";
            AvailablePercentage = $"{(AvailableRooms * 100 / TotalRooms)}%";
            MaintenancePercentage = $"{(MaintenanceRooms * 100 / TotalRooms)}%";
            OutOfServicePercentage = $"{(OutOfServiceRooms * 100 / TotalRooms)}%";

            RoomStatusSeries = new ISeries[]
            {
                new PieSeries<int> { Values = new[] { OccupiedRooms }, Name = "Occupied", Fill = new SolidColorPaint(SKColor.Parse("#3B82F6")), InnerRadius = 55 },
                new PieSeries<int> { Values = new[] { AvailableRooms }, Name = "Available", Fill = new SolidColorPaint(SKColor.Parse("#10B981")), InnerRadius = 55 },
                new PieSeries<int> { Values = new[] { MaintenanceRooms }, Name = "Maintenance", Fill = new SolidColorPaint(SKColor.Parse("#F59E0B")), InnerRadius = 55 },
                new PieSeries<int> { Values = new[] { OutOfServiceRooms }, Name = "Out of Service", Fill = new SolidColorPaint(SKColor.Parse("#EF4444")), InnerRadius = 55 }
            };
        }

        // Chart
        SetupOccupancyChart(reservations, startDate, today);

        // Recent
        var recent = reservations.OrderByDescending(r => r.Id).Take(5).ToList();
        RecentReservations = new ObservableCollection<Reservation>(recent);

        // Schedule
        var schedule = new List<ScheduleItem>();
        var todaysCheckIns = reservations.Where(r => r.CheckInDate.Date == today).ToList();
        foreach (var r in todaysCheckIns)
            schedule.Add(new ScheduleItem { Time = "12:00 PM", Title = $"Check-in: {r.Guest?.Name ?? "Guest"}", Subtitle = $"Room {r.Room?.RoomNumber ?? ""}", Icon = "DoorOpen", IconColor = "#3B82F6" });

        var todaysCheckOuts = reservations.Where(r => r.CheckOutDate.Date == today).ToList();
        foreach (var r in todaysCheckOuts)
            schedule.Add(new ScheduleItem { Time = "11:00 AM", Title = $"Check-out: {r.Guest?.Name ?? "Guest"}", Subtitle = $"Room {r.Room?.RoomNumber ?? ""}", Icon = "LogoutVariant", IconColor = "#10B981" });

        if(!schedule.Any())
            schedule.Add(new ScheduleItem { Time = "All Day", Title = "No events scheduled", Subtitle = "Your day is clear!", Icon = "CalendarCheck", IconColor = "#64748B" });

        TodaySchedule = new ObservableCollection<ScheduleItem>(schedule.OrderBy(s => s.Time));
    }

    private string CalculateTrend(double current, double previous, out bool isPositive)
    {
        if (previous == 0)
        {
            isPositive = true;
            return current > 0 ? "100%" : "0%";
        }
        var diff = ((current - previous) / previous) * 100;
        isPositive = diff >= 0;
        return $"{Math.Abs(diff):F0}%";
    }

    private void SetupOccupancyChart(List<Reservation> reservations, DateTime start, DateTime end)
    {
        var days = (end - start).Days;
        if (days == 0) days = 1;
        var labels = new List<string>();
        var values = new List<int>();

        for (int i = 0; i <= days; i++)
        {
            var d = start.AddDays(i);
            labels.Add(d.ToString("MMM dd"));
            values.Add(reservations.Count(r => r.CheckInDate.Date <= d && r.CheckOutDate.Date >= d));
        }

        OccupancySeries = new ISeries[]
        {
            new LineSeries<int>
            {
                Values = values.ToArray(),
                Name = "Occupied Rooms",
                Fill = new SolidColorPaint(SKColor.Parse("#333B82F6")),
                Stroke = new SolidColorPaint(SKColor.Parse("#3B82F6")) { StrokeThickness = 3 },
                GeometrySize = 8,
                GeometryFill = new SolidColorPaint(SKColors.White),
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#3B82F6")) { StrokeThickness = 2 }
            }
        };

        XAxes = new Axis[]
        {
            new Axis
            {
                Labels = labels.ToArray(),
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                TextSize = 11
            }
        };

        YAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                TextSize = 11,
                MinStep = 1
            }
        };
    }
}
