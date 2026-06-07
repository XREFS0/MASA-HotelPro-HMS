using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasaHotelPro.Models;
using MasaHotelPro.Repositories;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MasaHotelPro.ViewModels;

public partial class BillingViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private System.Collections.Generic.List<Invoice> _allInvoices = new();

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
            Invoices = new ObservableCollection<Invoice>(_allInvoices);
        }
        else
        {
            var q = SearchQuery.ToLower();
            Invoices = new ObservableCollection<Invoice>(
                _allInvoices.Where(x => (x.InvoiceNumber != null && x.InvoiceNumber.ToLower().Contains(q)) || 
                                        (x.Reservation != null && x.Reservation.Guest != null && x.Reservation.Guest.Name != null && x.Reservation.Guest.Name.ToLower().Contains(q)) ||
                                        (x.Status != null && x.Status.ToLower().Contains(q)))
            );
        }
    }

    [ObservableProperty]
    private ObservableCollection<Invoice> _invoices = new();

    [ObservableProperty]
    private ObservableCollection<Reservation> _pendingReservations = new();

    [ObservableProperty]
    private Invoice _selectedInvoice;

    [ObservableProperty]
    private Invoice _invoiceForm;

    [ObservableProperty]
    private bool _isFormVisible;

    public BillingViewModel()
    {
    }

    public BillingViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var invoicesList = await _unitOfWork.Invoices.GetAllAsync();
        _allInvoices = invoicesList.ToList();
        
        var reservationsList = await _unitOfWork.Reservations.GetAllAsync();
        PendingReservations = new ObservableCollection<Reservation>(reservationsList);

        FilterData();
    }

    [RelayCommand]
    private void GenerateInvoice()
    {
        InvoiceForm = new Invoice
        {
            IssueDate = System.DateTime.Today,
            Status = "Unpaid"
        };
        IsFormVisible = true;
    }

    [RelayCommand]
    private void EditInvoice()
    {
        if (SelectedInvoice == null) return;
        
        InvoiceForm = new Invoice
        {
            Id = SelectedInvoice.Id,
            InvoiceNumber = SelectedInvoice.InvoiceNumber,
            Reservation = SelectedInvoice.Reservation,
            TotalAmount = SelectedInvoice.TotalAmount,
            IssueDate = SelectedInvoice.IssueDate,
            Status = SelectedInvoice.Status
        };
        IsFormVisible = true;
    }

    [RelayCommand]
    private async Task SaveInvoiceAsync()
    {
        if (InvoiceForm == null || InvoiceForm.Reservation == null) return;

        if (InvoiceForm.Id == 0)
        {
            InvoiceForm.InvoiceNumber = "INV-" + System.Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            await _unitOfWork.Invoices.AddAsync(InvoiceForm);
        }
        else
        {
            var existingInv = await _unitOfWork.Invoices.GetByIdAsync(InvoiceForm.Id);
            if (existingInv != null)
            {
                existingInv.Reservation = InvoiceForm.Reservation;
                existingInv.TotalAmount = InvoiceForm.TotalAmount;
                existingInv.Status = InvoiceForm.Status;
                _unitOfWork.Invoices.Update(existingInv);
            }
        }
        
        await _unitOfWork.CompleteAsync();
        IsFormVisible = false;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task DeleteInvoiceAsync(Invoice invoice)
    {
        var targetInvoice = invoice ?? SelectedInvoice ?? InvoiceForm;
        if (targetInvoice != null && targetInvoice.Id != 0)
        {
            var existing = await _unitOfWork.Invoices.GetByIdAsync(targetInvoice.Id);
            if (existing != null)
            {
                _unitOfWork.Invoices.Remove(existing);
                await _unitOfWork.CompleteAsync();
                _ = LoadDataAsync();
                IsFormVisible = false;
            }
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        IsFormVisible = false;
        InvoiceForm = null;
    }
}

