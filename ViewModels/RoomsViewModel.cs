using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasaHotelPro.Models;
using MasaHotelPro.Repositories;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MasaHotelPro.ViewModels;

public partial class RoomsViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private System.Collections.Generic.List<Room> _allRooms = new();

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
            Rooms = new ObservableCollection<Room>(_allRooms);
        }
        else
        {
            var q = SearchQuery.ToLower();
            Rooms = new ObservableCollection<Room>(
                _allRooms.Where(x => (x.RoomNumber != null && x.RoomNumber.ToLower().Contains(q)) || 
                                     (x.Type != null && x.Type.ToLower().Contains(q)))
            );
        }
    }

    [ObservableProperty]
    private ObservableCollection<Room> _rooms = new();

    [ObservableProperty]
    private Room _selectedRoom;

    [ObservableProperty]
    private Room _roomForm;

    [ObservableProperty]
    private bool _isFormVisible;

    public RoomsViewModel()
    {
        // Design-time
    }

    public RoomsViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var rooms = await _unitOfWork.Rooms.GetAllAsync();
        _allRooms = rooms.ToList();
        FilterData();
    }

    [RelayCommand]
    private void AddRoom()
    {
        RoomForm = new Room();
        IsFormVisible = true;
    }

    [RelayCommand]
    private void EditRoom()
    {
        if (SelectedRoom == null) return;
        
        // Clone the selected room for editing
        RoomForm = new Room
        {
            Id = SelectedRoom.Id,
            RoomNumber = SelectedRoom.RoomNumber,
            Name = SelectedRoom.Name,
            Type = SelectedRoom.Type,
            Status = SelectedRoom.Status,
            Price = SelectedRoom.Price
        };
        IsFormVisible = true;
    }

    [RelayCommand]
    private async Task DeleteRoomAsync(Room room)
    {
        var targetRoom = room ?? SelectedRoom ?? RoomForm;
        if (targetRoom != null && targetRoom.Id != 0)
        {
            var existing = await _unitOfWork.Rooms.GetByIdAsync(targetRoom.Id);
            if (existing != null)
            {
                _unitOfWork.Rooms.Remove(existing);
                await _unitOfWork.CompleteAsync();
                _ = LoadDataAsync();
                IsFormVisible = false;
            }
        }
    }

    [RelayCommand]
    private async Task SaveRoomAsync()
    {
        if (RoomForm == null) return;

        if (RoomForm.Id == 0)
        {
            await _unitOfWork.Rooms.AddAsync(RoomForm);
        }
        else
        {
            var existingRoom = await _unitOfWork.Rooms.GetByIdAsync(RoomForm.Id);
            if (existingRoom != null)
            {
                existingRoom.RoomNumber = RoomForm.RoomNumber;
                existingRoom.Name = RoomForm.Name;
                existingRoom.Type = RoomForm.Type;
                existingRoom.Status = RoomForm.Status;
                existingRoom.Price = RoomForm.Price;
                _unitOfWork.Rooms.Update(existingRoom);
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
        RoomForm = null;
    }
}

