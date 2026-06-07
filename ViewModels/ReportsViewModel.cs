using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasaHotelPro.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MasaHotelPro.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportsViewModel(IUnitOfWork unitOfWork) 
    { 
        _unitOfWork = unitOfWork;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [RelayCommand]
    private async Task GenerateFinancialReportAsync()
    {
        try
        {
            var invoices = await _unitOfWork.Invoices.GetAllAsync();
            var totalRevenue = invoices.Sum(i => i.TotalAmount);
            var totalPaid = invoices.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount);

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, $"Financial_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader("Financial Report"));
                    page.Content().Element(x => 
                    {
                        x.Column(column => 
                        {
                            column.Spacing(20);
                            column.Item().Text($"Total Revenue: {totalRevenue:C}").FontSize(14).SemiBold();
                            column.Item().Text($"Total Paid: {totalPaid:C}").FontSize(14).SemiBold();
                            column.Item().Text($"Total Unpaid: {totalRevenue - totalPaid:C}").FontSize(14).SemiBold();
                            column.Item().Text($"Number of Invoices: {invoices.Count()}");
                        });
                    });
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf(filePath);

            OpenFile(filePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task GenerateOccupancyReportAsync()
    {
        try
        {
            var rooms = await _unitOfWork.Rooms.GetAllAsync();
            var totalRooms = rooms.Count();
            var occupiedRooms = rooms.Count(r => r.Status == Models.RoomStatus.Occupied);
            var occupancyRate = totalRooms > 0 ? (double)occupiedRooms / totalRooms * 100 : 0;

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, $"Occupancy_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader("Occupancy Report"));
                    page.Content().Element(x => 
                    {
                        x.Column(column => 
                        {
                            column.Spacing(20);
                            column.Item().Text($"Total Rooms: {totalRooms}").FontSize(14);
                            column.Item().Text($"Occupied Rooms: {occupiedRooms}").FontSize(14);
                            column.Item().Text($"Available Rooms: {totalRooms - occupiedRooms}").FontSize(14);
                            column.Item().Text($"Current Occupancy Rate: {occupancyRate:F2}%").FontSize(16).SemiBold().FontColor(Colors.Blue.Darken2);
                        });
                    });
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf(filePath);

            OpenFile(filePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating report: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task GenerateGuestDemographicsAsync()
    {
        try
        {
            var guests = await _unitOfWork.Guests.GetAllAsync();
            var totalGuests = guests.Count();

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, $"Guest_Demographics_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader("Guest Demographics"));
                    page.Content().Element(x => 
                    {
                        x.Column(column => 
                        {
                            column.Spacing(20);
                            column.Item().Text($"Total Registered Guests: {totalGuests}").FontSize(14).SemiBold();
                            
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Nationality").SemiBold();
                                    header.Cell().Text("Count").SemiBold();
                                });

                                var nationalities = guests.GroupBy(g => g.Nationality).Select(g => new { Nationality = g.Key ?? "Unknown", Count = g.Count() }).OrderByDescending(g => g.Count);
                                foreach (var nat in nationalities)
                                {
                                    table.Cell().Text(nat.Nationality);
                                    table.Cell().Text(nat.Count.ToString());
                                }
                            });
                        });
                    });
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf(filePath);

            OpenFile(filePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating report: {ex.Message}");
        }
    }

    [RelayCommand]
    private void GenerateHousekeepingReport()
    {
        try
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, $"Housekeeping_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader("Housekeeping & Maintenance"));
                    page.Content().Element(x => 
                    {
                        x.Column(column => 
                        {
                            column.Spacing(20);
                            column.Item().Text("Housekeeping module is partially implemented.").FontSize(12);
                            column.Item().Text("Detailed tasks and employee assignments will be included in the next update.").FontSize(12).FontColor(Colors.Grey.Medium);
                        });
                    });
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf(filePath);

            OpenFile(filePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating report: {ex.Message}");
        }
    }

    private Action<IContainer> ComposeHeader(string title)
    {
        return container =>
        {
            container.PaddingBottom(20).Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("MASA HotelPro HMS").FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text(title).FontSize(16).FontColor(Colors.Grey.Darken2);
                    column.Item().Text($"Generated on: {DateTime.Now:MMMM dd, yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium);
                });
            });
        };
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Page ");
            x.CurrentPageNumber();
            x.Span(" of ");
            x.TotalPages();
        });
    }

    private void OpenFile(string filePath)
    {
        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
    }
}

