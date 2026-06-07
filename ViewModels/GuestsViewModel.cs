using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasaHotelPro.Models;
using MasaHotelPro.Repositories;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MasaHotelPro.ViewModels;

public partial class GuestsViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private System.Collections.Generic.List<Guest> _allGuests = new();

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
            Guests = new ObservableCollection<Guest>(_allGuests);
        }
        else
        {
            var q = SearchQuery.ToLower();
            Guests = new ObservableCollection<Guest>(
                _allGuests.Where(x => (x.Name != null && x.Name.ToLower().Contains(q)) || 
                                      (x.Phone != null && x.Phone.ToLower().Contains(q)) ||
                                      (x.Email != null && x.Email.ToLower().Contains(q)))
            );
        }
    }

    [ObservableProperty]
    private ObservableCollection<Guest> _guests = new();

    [ObservableProperty]
    private Guest _selectedGuest;

    [ObservableProperty]
    private Guest _guestForm;

    [ObservableProperty]
    private bool _isFormVisible;

    public GuestsViewModel()
    {
    }

    public GuestsViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var guestsList = await _unitOfWork.Guests.GetAllAsync();
        _allGuests = guestsList.ToList();
        FilterData();
    }

    [RelayCommand]
    private void AddGuest()
    {
        GuestForm = new Guest();
        IsFormVisible = true;
    }

    [RelayCommand]
    private void EditGuest()
    {
        if (SelectedGuest == null) return;
        
        GuestForm = new Guest
        {
            Id = SelectedGuest.Id,
            Name = SelectedGuest.Name,
            Phone = SelectedGuest.Phone,
            Email = SelectedGuest.Email,
            Nationality = SelectedGuest.Nationality,
            DocumentNumber = SelectedGuest.DocumentNumber,
            Address = SelectedGuest.Address,
            Notes = SelectedGuest.Notes
        };
        IsFormVisible = true;
    }

    [RelayCommand]
    private async Task DeleteGuestAsync(Guest guest)
    {
        var targetGuest = guest ?? SelectedGuest ?? GuestForm;
        if (targetGuest != null && targetGuest.Id != 0)
        {
            var existing = await _unitOfWork.Guests.GetByIdAsync(targetGuest.Id);
            if (existing != null)
            {
                _unitOfWork.Guests.Remove(existing);
                await _unitOfWork.CompleteAsync();
                _ = LoadDataAsync();
                IsFormVisible = false;
            }
        }
    }

    [RelayCommand]
    private async Task SaveGuestAsync()
    {
        if (GuestForm == null) return;

        if (GuestForm.Id == 0)
        {
            await _unitOfWork.Guests.AddAsync(GuestForm);
        }
        else
        {
            var existingGuest = await _unitOfWork.Guests.GetByIdAsync(GuestForm.Id);
            if (existingGuest != null)
            {
                existingGuest.Name = GuestForm.Name;
                existingGuest.Phone = GuestForm.Phone;
                existingGuest.Email = GuestForm.Email;
                existingGuest.Nationality = GuestForm.Nationality;
                existingGuest.DocumentNumber = GuestForm.DocumentNumber;
                existingGuest.Address = GuestForm.Address;
                existingGuest.Notes = GuestForm.Notes;
                _unitOfWork.Guests.Update(existingGuest);
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
        GuestForm = null;
    }
}

