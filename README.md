# MASA HotelPro HMS

MASA HotelPro HMS is a comprehensive, modern Hotel Management System built with WPF and .NET 8.0. Designed to streamline hotel operations, it offers an intuitive and responsive user interface combined with a robust backend architecture to manage reservations, guests, rooms, housekeeping, and billing efficiently.

## Features

- **Dynamic Dashboard**: Real-time analytics and statistics including room status, occupancy rates, and revenue trends powered by LiveChartsCore.
- **Reservation Management**: Streamlined check-in and check-out processes, booking creation, and status tracking.
- **Room Management**: Keep track of room statuses (Occupied, Available, Maintenance, Out of Service) and room categories.
- **Guest Profiles**: Comprehensive guest database for managing contact information and booking history.
- **Housekeeping Module**: Task assignment and tracking for the housekeeping staff to ensure rooms are always ready.
- **Billing & Invoices**: Generate, manage, and track invoices and payments seamlessly.
- **Staff Management**: Centralized hub for managing hotel employees and their roles.

## Tech Stack

- **Framework**: .NET 8.0 (Windows Presentation Foundation)
- **Architecture**: MVVM (Model-View-ViewModel)
- **Core Libraries**:
  - CommunityToolkit.Mvvm (for robust MVVM implementation and messaging)
  - LiveChartsCore.SkiaSharpView.WPF (for advanced data visualization)
  - MaterialDesignThemes (for modern, material UI components)

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 (recommended) or any compatible IDE

### Installation & Build

1. Clone the repository to your local machine.
2. Open the solution file (`MasaHotelPro.sln` or `.csproj`) in your preferred IDE.
3. Restore the NuGet packages.
4. Build the solution to compile the application.
5. Run the application.

## Architecture Highlights

The application strictly follows the MVVM pattern, ensuring a clean separation of concerns:
- **Models**: Represent the core data structures and entities.
- **ViewModels**: Handle the presentation logic, state management, and communication between Views and Models using the repository pattern (`IUnitOfWork`).
- **Views**: XAML-based user interfaces styled with Material Design principles.

Cross-view navigation and communication are handled cleanly via the `WeakReferenceMessenger` from the MVVM Toolkit, preventing tight coupling between UI components.

## License

Copyright © 2026 Mohamed Mamdouh (MASA).

All rights reserved.

This software and its source code are the intellectual property of Mohamed Mamdouh (MASA). No part of this project may be copied, modified, distributed, or used for commercial purposes without prior written permission from the author.
