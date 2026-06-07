using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasaHotelPro.Models;
using MasaHotelPro.Repositories;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MasaHotelPro.ViewModels;

public partial class ReservationsViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private System.Collections.Generic.List<Reservation> _allReservations = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    partial void OnSearchQueryChanged(string value)
    {
        FilterData();
    }

    private void FilterData()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            Reservations = new ObservableCollection<Reservation>(_allReservations);
        }
        else
        {
            var q = SearchQuery.ToLower();
            Reservations = new ObservableCollection<Reservation>(
                _allReservations.Where(x => (x.Guest != null && x.Guest.Name != null && x.Guest.Name.ToLower().Contains(q)) || 
                                            (x.Status.ToString().ToLower().Contains(q)) ||
                                            (x.Room != null && x.Room.RoomNumber != null && x.Room.RoomNumber.ToLower().Contains(q)))
            );
        }
    }

    [ObservableProperty]
    private ObservableCollection<Reservation> _reservations = new();

    [ObservableProperty]
    private ObservableCollection<Guest> _availableGuests = new();

    [ObservableProperty]
    private ObservableCollection<Room> _availableRooms = new();

    [ObservableProperty]
    private Reservation _selectedReservation;

    [ObservableProperty]
    private Reservation _reservationForm;

    [ObservableProperty]
    private bool _isFormVisible;

    public ReservationsViewModel()
    {
    }

    public ReservationsViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var reservationsList = await _unitOfWork.Reservations.GetAllAsync();
        _allReservations = reservationsList.ToList();
        
        var roomsList = await _unitOfWork.Rooms.GetAllAsync();
        AvailableRooms = new ObservableCollection<Room>(roomsList.Where(r => r.Status == RoomStatus.Available));
        
        var guestsList = await _unitOfWork.Guests.GetAllAsync();
        AvailableGuests = new ObservableCollection<Guest>(guestsList);

        FilterData();
    }

    [RelayCommand]
    private void AddReservation()
    {
        ReservationForm = new Reservation
        {
            CheckInDate = System.DateTime.Today,
            CheckOutDate = System.DateTime.Today.AddDays(1),
            Status = ReservationStatus.Pending
        };
        IsFormVisible = true;
    }

    [RelayCommand]
    private void EditReservation()
    {
        if (SelectedReservation == null) return;
        
        ReservationForm = new Reservation
        {
            Id = SelectedReservation.Id,
            Guest = SelectedReservation.Guest,
            Room = SelectedReservation.Room,
            CheckInDate = SelectedReservation.CheckInDate,
            CheckOutDate = SelectedReservation.CheckOutDate,
            Status = SelectedReservation.Status,
            TotalPrice = SelectedReservation.TotalPrice
        };
        IsFormVisible = true;
    }

    [RelayCommand]
    private async Task DeleteReservationAsync(Reservation reservation)
    {
        var targetRes = reservation ?? SelectedReservation ?? ReservationForm;
        if (targetRes != null && targetRes.Id != 0)
        {
            var existing = await _unitOfWork.Reservations.GetByIdAsync(targetRes.Id);
            if (existing != null)
            {
                _unitOfWork.Reservations.Remove(existing);
                await _unitOfWork.CompleteAsync();
                _ = LoadDataAsync();
                IsFormVisible = false;
            }
        }
    }

    [RelayCommand]
    private async Task SaveReservationAsync()
    {
        if (ReservationForm == null || ReservationForm.Guest == null || ReservationForm.Room == null) return;

        if (ReservationForm.Id == 0)
        {
            ReservationForm.ReservationNumber = "RES-" + System.Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            await _unitOfWork.Reservations.AddAsync(ReservationForm);
        }
        else
        {
            var existingRes = await _unitOfWork.Reservations.GetByIdAsync(ReservationForm.Id);
            if (existingRes != null)
            {
                existingRes.Guest = ReservationForm.Guest;
                existingRes.Room = ReservationForm.Room;
                existingRes.CheckInDate = ReservationForm.CheckInDate;
                existingRes.CheckOutDate = ReservationForm.CheckOutDate;
                existingRes.Status = ReservationForm.Status;
                existingRes.TotalPrice = ReservationForm.TotalPrice;
                _unitOfWork.Reservations.Update(existingRes);
            }
        }
        
        await _unitOfWork.CompleteAsync();
        IsFormVisible = false;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private void Cancel()
    {
        IsFormVisible = false;
        ReservationForm = null;
    }
}

