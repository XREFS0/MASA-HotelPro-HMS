using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasaHotelPro.Models;
using MasaHotelPro.Repositories;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.Linq;

namespace MasaHotelPro.ViewModels;

public partial class EmployeesViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private System.Collections.Generic.List<Employee> _allEmployees = new();

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
            Employees = new ObservableCollection<Employee>(_allEmployees);
        }
        else
        {
            var q = SearchQuery.ToLower();
            Employees = new ObservableCollection<Employee>(
                _allEmployees.Where(x => (x.Name != null && x.Name.ToLower().Contains(q)) || 
                                         (x.JobTitle != null && x.JobTitle.ToLower().Contains(q)))
            );
        }
    }

    [ObservableProperty]
    private ObservableCollection<Employee> _employees = new();

    [ObservableProperty]
    private Employee? _selectedEmployee;

    [ObservableProperty]
    private Employee _employeeForm = new();

    [ObservableProperty]
    private bool _isFormVisible;

    public EmployeesViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var employeesList = await _unitOfWork.Employees.GetAllAsync();
        _allEmployees = employeesList.ToList();
        FilterData();
    }

    [RelayCommand]
    private void AddNewEmployee()
    {
        EmployeeForm = new Employee { Name = string.Empty, JobTitle = string.Empty, Phone = string.Empty };
        SelectedEmployee = null;
        IsFormVisible = true;
    }

    [RelayCommand]
    private void EditEmployee()
    {
        if (SelectedEmployee != null)
        {
            EmployeeForm = new Employee
            {
                Id = SelectedEmployee.Id,
                Name = SelectedEmployee.Name,
                JobTitle = SelectedEmployee.JobTitle,
                Phone = SelectedEmployee.Phone
            };
            IsFormVisible = true;
        }
    }

    [RelayCommand]
    private async Task SaveEmployeeAsync()
    {
        if (string.IsNullOrWhiteSpace(EmployeeForm.Name)) return;

        try
        {
            if (EmployeeForm.Id == 0)
            {
                await _unitOfWork.Employees.AddAsync(EmployeeForm);
            }
            else
            {
                var trackedEmployee = await _unitOfWork.Employees.GetByIdAsync(EmployeeForm.Id);
                if (trackedEmployee != null)
                {
                    trackedEmployee.Name = EmployeeForm.Name;
                    trackedEmployee.JobTitle = EmployeeForm.JobTitle;
                    trackedEmployee.Phone = EmployeeForm.Phone;
                    _unitOfWork.Employees.Update(trackedEmployee);
                }
            }

            await _unitOfWork.CompleteAsync();
            await LoadDataAsync();
            IsFormVisible = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save. Error: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteEmployeeAsync(Employee employee)
    {
        if (employee == null || employee.Id == 0) return;
        
        try
        {
            var trackedEmployee = await _unitOfWork.Employees.GetByIdAsync(employee.Id);
            if (trackedEmployee == null) return;

            var relatedTasks = await _unitOfWork.HousekeepingTasks.FindAsync(t => t.EmployeeId == trackedEmployee.Id);
            if (relatedTasks != null && relatedTasks.Any())
            {
                _unitOfWork.HousekeepingTasks.RemoveRange(relatedTasks);
            }

            _unitOfWork.Employees.Remove(trackedEmployee);
            await _unitOfWork.CompleteAsync();
            await LoadDataAsync();
            IsFormVisible = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to delete. Unexpected error: {ex.Message}", "Deletion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            // Reload data to revert any local changes that weren't saved to DB
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        IsFormVisible = false;
        EmployeeForm = new Employee { Name = string.Empty, JobTitle = string.Empty, Phone = string.Empty };
        SelectedEmployee = null;
    }
}
