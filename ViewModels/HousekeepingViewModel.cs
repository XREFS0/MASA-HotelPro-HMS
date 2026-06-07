using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasaHotelPro.Models;
using MasaHotelPro.Repositories;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System;

namespace MasaHotelPro.ViewModels;

public partial class HousekeepingViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private System.Collections.Generic.List<HousekeepingTask> _allTasks = new();

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
            Tasks = new ObservableCollection<HousekeepingTask>(_allTasks);
        }
        else
        {
            var q = SearchQuery.ToLower();
            Tasks = new ObservableCollection<HousekeepingTask>(
                _allTasks.Where(x => (x.Room != null && x.Room.RoomNumber != null && x.Room.RoomNumber.ToLower().Contains(q)) || 
                                     (x.Employee != null && x.Employee.Name != null && x.Employee.Name.ToLower().Contains(q)) ||
                                     (x.Status != null && x.Status.ToLower().Contains(q)))
            );
        }
    }

    [ObservableProperty]
    private ObservableCollection<HousekeepingTask> _tasks = new();

    [ObservableProperty]
    private ObservableCollection<Room> _rooms = new();

    [ObservableProperty]
    private ObservableCollection<Employee> _employees = new();

    [ObservableProperty]
    private HousekeepingTask? _selectedTask;

    [ObservableProperty]
    private HousekeepingTask _taskForm = new();

    [ObservableProperty]
    private bool _isFormVisible;

    public HousekeepingViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var tasksList = await _unitOfWork.HousekeepingTasks.GetAllAsync();
        _allTasks = tasksList.ToList();

        var rooms = await _unitOfWork.Rooms.GetAllAsync();
        Rooms = new ObservableCollection<Room>(rooms);

        var employees = await _unitOfWork.Employees.GetAllAsync();
        if (!employees.Any())
        {
            await _unitOfWork.Employees.AddAsync(new Employee { Name = "Ahmed Ali", JobTitle = "Cleaner", Phone = "0100000001" });
            await _unitOfWork.Employees.AddAsync(new Employee { Name = "Sara Youssef", JobTitle = "Cleaner", Phone = "0100000002" });
            await _unitOfWork.CompleteAsync();
            employees = await _unitOfWork.Employees.GetAllAsync();
        }
        Employees = new ObservableCollection<Employee>(employees);

        FilterData();
    }

    [RelayCommand]
    private void AddNewTask()
    {
        TaskForm = new HousekeepingTask { ScheduledTime = System.DateTime.Now, Status = "Pending" };
        SelectedTask = null;
        IsFormVisible = true;
    }

    [RelayCommand]
    private void EditTask()
    {
        if (SelectedTask != null)
        {
            TaskForm = new HousekeepingTask
            {
                Id = SelectedTask.Id,
                RoomId = SelectedTask.RoomId,
                Room = SelectedTask.Room,
                EmployeeId = SelectedTask.EmployeeId,
                Employee = SelectedTask.Employee,
                ScheduledTime = SelectedTask.ScheduledTime,
                Status = SelectedTask.Status
            };
            IsFormVisible = true;
        }
    }

    [RelayCommand]
    private async Task SaveTaskAsync()
    {
        if (TaskForm.Room == null || TaskForm.Employee == null) return;

        try
        {
            TaskForm.RoomId = TaskForm.Room.Id;
            TaskForm.EmployeeId = TaskForm.Employee.Id;

            if (TaskForm.Id == 0)
            {
                await _unitOfWork.HousekeepingTasks.AddAsync(TaskForm);
            }
            else
            {
                var trackedTask = await _unitOfWork.HousekeepingTasks.GetByIdAsync(TaskForm.Id);
                if (trackedTask != null)
                {
                    trackedTask.RoomId = TaskForm.RoomId;
                    trackedTask.EmployeeId = TaskForm.EmployeeId;
                    trackedTask.ScheduledTime = TaskForm.ScheduledTime;
                    trackedTask.Status = TaskForm.Status;
                    _unitOfWork.HousekeepingTasks.Update(trackedTask);
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
    private async Task DeleteTaskAsync(HousekeepingTask task)
    {
        if (task == null || task.Id == 0) return;
        
        try
        {
            var trackedTask = await _unitOfWork.HousekeepingTasks.GetByIdAsync(task.Id);
            if (trackedTask != null)
            {
                _unitOfWork.HousekeepingTasks.Remove(trackedTask);
                await _unitOfWork.CompleteAsync();
            }
            
            await LoadDataAsync();
            IsFormVisible = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to delete. Error: {ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        IsFormVisible = false;
        TaskForm = new HousekeepingTask();
        SelectedTask = null;
    }
}
