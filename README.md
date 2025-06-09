
# AuctionX — Real-Time Player Auction Platform

AuctionX is a real-time player auction web application designed for fantasy leagues, sports tournaments, and any scenario requiring fast-paced live bidding. Built from the ground up using ASP.NET Core MVC and SignalR, AuctionX enables multiple bidders to compete in real-time while providing organizers full control over auction events.

---

## Features

- 🎥 **Real-Time Auction Room** powered by SignalR for instant bid updates across all connected clients  
- 🧑‍💼 **Role-Based Access Control**: Only the tournament organizer can start, pause, and finalize auctions  
- 📊 **Live Player Status**: SOLD / UNSOLD statuses update instantly and are visible to all users  
- 👥 **Dynamic Team Builder**: Organizers can generate teams on the fly and assign players dynamically during the auction  
- 🔔 **Real-Time Notifications**: All users receive immediate updates for bids and auction status changes  
- 📱 **Responsive Design**: Accessible from desktop, tablet, and mobile devices  

---

## Technology Stack

- **Backend:** ASP.NET Core MVC (.NET 8)  
- **Real-Time Communication:** SignalR  
- **Database:** SQL Server (LocalDB for development)  
- **Frontend:** Razor Views with Bootstrap for responsiveness  

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)  
- [SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)  
- Visual Studio 2022 or any IDE supporting ASP.NET Core MVC development  

---

### Setup Instructions

1. **Clone the repository:**
   ```bash
   https://github.com/HarriiM04/AuctionX.git
   cd AuctionX


2. **Configure the database connection string:**

   * Update `appsettings.json` with your local SQL Server LocalDB connection string:

     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=AuctionXDb;Trusted_Connection=True;"
     }
     ```

3. **Run database migrations (if applicable):**

   * Use EF Core migrations or your preferred method to create the database schema.

4. **Build and run the project:**

   ```bash
   dotnet build
   dotnet run
   ```

5. **Access the app:**

   * Open your browser and navigate to `https://localhost:5001` (or your configured port).

---

## Usage Overview

* **Organizer Role:**

  * Create tournaments and add players.
  * Start, pause, and finalize auctions exclusively for tournaments they created.
  * Dynamically add teams and assign players.

* **Bidder Role:**

  * Join live auctions and place bids in real-time.
  * View current bid status and SOLD/UNSOLD player lists.

---

## Application Architecture and Flow

### Overview

AuctionX follows a clean **MVC architecture** with distinct separation of concerns:

* **Models:** Represent data entities like Players, Teams, Tournaments, Bids, and Users.
* **Views:** Razor views render responsive UI components for auction rooms, player lists, team creation, and bidding interfaces.
* **Controllers:** Handle HTTP requests, enforce business logic, and manage role-based access control.

### Real-Time Communication

* **SignalR Hub:**
  Acts as the central communication channel for broadcasting live auction events:

  * New bids placed by bidders
  * Auction status changes (start, pause, finalize) initiated by organizers
  * Player SOLD/UNSOLD status updates

* **Client Interaction:**
  Connected clients receive push notifications in real-time, enabling synchronized views of auction progress.

### Auction Flow

1. **Setup:** Organizer creates a tournament, uploads players, and optionally creates teams dynamically.
2. **Auction Start:** Organizer starts the auction session; all bidders get notified.
3. **Bidding:** Bidders place bids on players; SignalR broadcasts each new bid to all participants.
4. **Player Sold:** Once bidding closes for a player, their status updates to SOLD and the information propagates to all clients instantly.
5. **Auction Pause/Finalize:** Organizer can pause or finalize the auction, controlling the flow and ending the session accordingly.

---

## Future Enhancements

* Dockerization of full web app
* Cloud deployment for broader access
* Advanced analytics dashboard to track bidding patterns and tournament statistics

---

## Contributing

Contributions, feature requests, and bug reports are welcome! Please open an issue or submit a pull request.
